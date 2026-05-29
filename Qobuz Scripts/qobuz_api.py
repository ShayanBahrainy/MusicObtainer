
import time
import hashlib
import requests

app_id = "798273057"
user_auth = "KFBQWcCEfRfby8jd7cSk1LKiMW5-3D3vUa1wX7b4xQ6tozgO9LQlh_pOkf6_nm89TpnXUwIChOJrp5vGvahz9g"
app_secret = "abb21364945c0583309667d13ca3d93a"


def get_track_url(track_id: int) -> str:
    unix = int(time.time())

    format_id = 27

    headers = {
        "X-App-Id" : app_id,
        "X-User-Auth-Token" : user_auth,
        "User-Agent" : "Mozilla/5.0 (X11; Linux x86_64; rv:138.0) Gecko/20100101 Firefox/138.0"
    }

    r_sig = "trackgetFileUrlformat_id{}intentstreamtrack_id{}{}{}".format(format_id, track_id, unix, app_secret)
    r_sig_hashed = hashlib.md5(r_sig.encode("utf-8")).hexdigest()

    params = {
        "request_ts" : unix,
        "request_sig" : r_sig_hashed,
        "track_id" : track_id,
        "format_id" : format_id,
        "intent" : "stream",
    }

    url = "https://www.qobuz.com/api.json/0.2/track/getFileUrl"
    r = requests.get(url, params=params, headers=headers)
    if r.ok:
        response_json = r.json()
        print(f"format_id: {response_json.get('format_id')}, mime: {response_json.get('mime_type')}, restrictions: {response_json.get('restrictions')}")
        return response_json['url']
    else:
        raise LookupError(r.status_code, r.json())

def get_track_url(track_id: int, format_id: int = 27) -> tuple[str, int]:
    unix = int(time.time())
    r_sig = "trackgetFileUrlformat_id{}intentstreamtrack_id{}{}{}".format(format_id, track_id, unix, app_secret)
    r_sig_hashed = hashlib.md5(r_sig.encode("utf-8")).hexdigest()
    params = {
        "request_ts": unix,
        "request_sig": r_sig_hashed,
        "track_id": track_id,
        "format_id": format_id,
        "intent": "stream",
    }

    headers = {
        "X-App-Id" : app_id,
        "X-User-Auth-Token" : user_auth,
        "User-Agent" : "Mozilla/5.0 (X11; Linux x86_64; rv:138.0) Gecko/20100101 Firefox/138.0"
    }

    r = requests.get("https://www.qobuz.com/api.json/0.2/track/getFileUrl", params=params, headers=headers)

    if not r.ok:
        raise LookupError(r.status_code, r.json())
    
    data = r.json()
    actual_format = data.get("format_id", format_id)
    restrictions = data.get("restrictions", [])
    restricted = any(r.get("code") == "FormatRestrictedByFormatAvailability" for r in restrictions)
    
    if restricted and format_id != 6:
        print(f"  ⚠️ format_id {format_id} unavailable for {track_id}, falling back to 6")
        return get_track_url(track_id, format_id=6)
    
    return data["url"], actual_format

def get_track_meta(track_id: int) -> str:
    headers = {
        "X-App-Id" : app_id,
        "X-User-Auth-Token" : user_auth,
        "User-Agent" : "Mozilla/5.0 (X11; Linux x86_64; rv:138.0) Gecko/20100101 Firefox/138.0"
    }

    params = {
        "track_id" : track_id,
    }

    url = "https://www.qobuz.com/api.json/0.2/track/get"
    r = requests.get(url, params=params, headers=headers)
    if r.ok:
        return r.json()
    else:
        raise LookupError(r.status_code, r.text)

def get_album_tracks(album_id: str) -> list[int]:
    headers = {
        "X-App-Id" : app_id,
        "X-User-Auth-Token" : user_auth,
        "User-Agent" : "Mozilla/5.0 (X11; Linux x86_64; rv:138.0) Gecko/20100101 Firefox/138.0"
    }

    params = {
        "album_id" : album_id,
        "app_id" : app_id,
        "offset" : 0,
        "limit" : 1200,
        "extra" : "track_ids,albumsFromSameArtist"
    }

    url = "https://www.qobuz.com/api.json/0.2/album/get"


    r = requests.get(url=url, params=params, headers=headers)
    if not r.ok:
        raise LookupError(r.status_code)

    ids = []
    for track in r.json()["tracks"]["items"]:
        ids.append(track["id"])

    return ids

#get_track_meta(121988687)
