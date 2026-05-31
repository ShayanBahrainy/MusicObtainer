import requests, os, shutil, re, asyncio, logging
from mutagen.flac import FLAC, Picture
from tempfile import NamedTemporaryFile
from qobuz_api import get_track_url, get_track_meta
from filelock import FileLock

OUTPUT_DIR = "/home/shayanbahrainy/Programming/Python/FLAC Sniffer/Qobuz Downloads"
SEEN_FILE  = os.path.join(OUTPUT_DIR, "index.txt")
LOCK_FILE  = SEEN_FILE + ".lock"


def setup(output_dir):
    global OUTPUT_DIR
    global SEEN_FILE
    global LOCK_FILE

    OUTPUT_DIR = output_dir
    SEEN_FILE  = os.path.join(OUTPUT_DIR, "index.txt")
    LOCK_FILE  = SEEN_FILE + ".lock"

DOWNLOAD_RETRIES = 3
RETRY_DELAY      = 2   
STREAM_CHUNK     = 65536

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  %(levelname)-8s  %(message)s",
    datefmt="%H:%M:%S",
)
log = logging.getLogger("flac-sniffer")

_lock = FileLock(LOCK_FILE)

def _load_seen() -> set:
    if not os.path.exists(SEEN_FILE):
        return set()
    with open(SEEN_FILE) as f:
        return {line.strip() for line in f if line.strip()}

def _mark_seen(track_id: str) -> None:
    with _lock:
        seen = _load_seen()
        if track_id in seen:
            return
        with open(SEEN_FILE, "a") as f:
            f.write(str(track_id) + "\n")

def _is_seen(track_id: str) -> bool:
    with _lock:
        return track_id in _load_seen()

def safe_name(name: str) -> str:
    name = re.sub(r'[<>:"/\\|?*\x00-\x1f]', ' ', name)
    return re.sub(r' {2,}', ' ', name).strip(" .")

def get_track_id_from_url(url: str) -> str | None:
    for param in url.split("?")[-1].split("&"):
        if param.startswith("track_id="):
            value = param[len("track_id="):]
            if value.isdigit():
                return value
    return None

def _download_with_retry(track_url: str) -> str:
    last_exc = None
    for attempt in range(1, DOWNLOAD_RETRIES + 1):
        tmp = None
        try:
            r = requests.get(track_url, stream=True, timeout=30)
            r.raise_for_status()
            with NamedTemporaryFile(delete=False, suffix=".flac") as tmp_f:
                tmp = tmp_f.name
                for chunk in r.iter_content(STREAM_CHUNK):
                    if chunk:
                        tmp_f.write(chunk)
            FLAC(tmp)
            return tmp
        except Exception as e:
            last_exc = e
            log.warning(f"  Attempt {attempt}/{DOWNLOAD_RETRIES} failed: {e}")
            if tmp and os.path.exists(tmp):
                os.remove(tmp)
            if attempt < DOWNLOAD_RETRIES:
                import time; time.sleep(RETRY_DELAY)
    raise RuntimeError(f"All {DOWNLOAD_RETRIES} download attempts failed") from last_exc

def _tag_flac(path: str, metadata: dict) -> None:
    audio = FLAC(path)
    audio["TITLE"]       = metadata["title"]
    audio["ARTIST"]      = metadata["performer"]["name"]
    audio["ALBUM"]       = metadata["album"]["title"]
    audio["TRACKNUMBER"] = str(metadata["track_number"])
    audio["DATE"]        = metadata["release_date_original"].split("-")[0]

    img_url = metadata["album"]["image"]["large"]
    img_r   = requests.get(img_url, timeout=15)
    img_r.raise_for_status()

    pic = Picture()
    pic.data = img_r.content
    pic.type = 3
    pic.mime = "image/jpeg"
    pic.desc = "Cover image for " + metadata["album"]["title"]
    audio.add_picture(pic)
    audio.save()


def process_song(track_id: str) -> int:
    "Nonzero return values mean an error occurred"
    if _is_seen(track_id):
        log.info(f"⏭  Already indexed, skipping {track_id}")
        return 0

    log.info(f"🎵 Processing track {track_id}")
    tmp_path = None

    try:
        track_url, actual_format = get_track_url(int(track_id))

        tmp_path = _download_with_retry(track_url)

        metadata = get_track_meta(int(track_id))
        _tag_flac(tmp_path, metadata)
        artist = metadata["performer"]["name"]
        album  = metadata["album"]["title"]
        title  = metadata["title"]

        dest_dir  = os.path.join(OUTPUT_DIR, safe_name(artist), safe_name(album))
        os.makedirs(dest_dir, exist_ok=True)
        dest_file = os.path.join(dest_dir, f"{safe_name(title)}.flac")

        if os.path.exists(dest_file):
            log.warning(f"  🟡 Destination already exists, skipping move.")
            os.remove(tmp_path)
            tmp_path = None
        else:
            shutil.move(tmp_path, dest_file)
            tmp_path = None
            log.info(f"  ✅ Saved: {dest_file}")

        _mark_seen(track_id)

        return 0

    except Exception as e:
        log.error(f"  ❌ Failed for track {track_id}: {e}", exc_info=True)
        if tmp_path and os.path.exists(tmp_path):
            try:
                os.remove(tmp_path)
            except OSError:
                pass
        return -1

def hello_world_test():
    print("Hello World")
    return 1