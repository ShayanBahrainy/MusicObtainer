# Music Obtainer
This is a Jellyfin plugin that will download music off of Qobuz and put it into your library. I use Qobuz for my music, but I got annoyed having to manually add music to my Jellyfin server to I made this :)


https://github.com/user-attachments/assets/9f5b4323-7b7d-48af-b220-aff9811a3d8b


# Configuring
This [repo](https://github.com/QobuzDL/Qobuz-AppID-Secret-Tool) will help you get the AppID and Secret for your configuration. To get your user authentication key, go to [Qobuz](https://play.qobuz.com), then open the Application menu of Dev tools, hit Local Storage, then the domain, and the key is "token" under `localuser`.

# Running
To run the project, you will need Python 3 installed, a Qobuz account, and these Python packages:
- Mutagen
- Filelock
- Requests

# Compiling
To compile the project, you will need .NET 9.0! 
Run `dotnet build`, this will produce two DLLs in the bin folder, both of these will need to be in a folder in the plugins folder of your Jellyfin to manually install.
