from qobuz_api import get_album_tracks
from process_songs import process_song
import time

if __name__ == "__main__":
    album_id = input("Enter album id: ")
    track_ids = get_album_tracks(album_id)

    for id in track_ids:
        process_song(id)
        time.sleep(2)
