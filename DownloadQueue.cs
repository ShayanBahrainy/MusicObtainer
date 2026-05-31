
using System.Collections.Generic;
using MediaBrowser.Model.Tasks;
using System.Threading.Tasks;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.Music.Configuration;
using Python.Runtime;

using System.Reflection;
using MediaBrowser.Model.Entities;
using System.Globalization;

namespace Jellyfin.Plugin.MusicObtainer;

/// <summary>
/// Task to download all items off queue
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
public partial class DownloadTask(ILogger<DownloadTask> logger) : IScheduledTask
{
    /// <summary>
    /// Name of Task
    /// </summary>
    public string Name { get; } = "Download queue items";

    /// <summary>
    /// Description of Task
    /// </summary>
    public string Description { get; } = "Download all items in the queue from Qobuz.";

    /// <summary>
    /// Task Key
    /// </summary>
    public string Key { get; } = "DownloadQueueItems";

    /// <summary>
    /// Category of Key
    /// </summary>
    public string Category { get; } = "Library";

    private readonly ILogger logger = logger;

    /// <summary>
    /// Returns default triggers for download task
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return [];
    }

    /// <summary>
    /// Log that the task is running.
    /// </summary>
    ///
    [LoggerMessage(
        EventId = 1,
        Message = "Downloading Qobuz items from queue..."
    )]
    static partial void LogTaskRun(ILogger logger, LogLevel level);

    static private void Log(ILogger logger, string message)
    {
        #pragma warning disable
        logger.LogInformation(message);
        #pragma warning restore
    }

    /// <summary>
    /// Executes task
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    { 
        var config = MusicObtainment.Instance!.Configuration;
        var libraryManager = MusicObtainment.Instance!.LibraryManager;
        await Task.Run(() => {
            using (Py.GIL()) {


                string? musicFolderPath = null;

                List<VirtualFolderInfo> virtualFolders = libraryManager.GetVirtualFolders();

                foreach (VirtualFolderInfo virtualFolder in virtualFolders)
                {
                    if (virtualFolder.CollectionType == CollectionTypeOptions.music)
                    {
                        musicFolderPath = virtualFolder.Locations[0];
                        break;
                    }
                }

                if (musicFolderPath == null) {
                    Log(logger, "No music collection found, exiting...");
                    return;
                }

                dynamic sys = Py.Import("sys");
                sys.path.append(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

                dynamic qobuzApi = Py.Import("qobuz_api");
                dynamic songProcessor = Py.Import("process_songs");

                songProcessor.setup(musicFolderPath);

                qobuzApi.setup(Convert.ToString(config.AppID, CultureInfo.InvariantCulture), config.UserAuth, config.AppSecret);


                QobuzQueueItem[] queueItems = new QobuzQueueItem[config.queueItems.Count];
                config.queueItems.CopyTo(queueItems);

                foreach (QobuzQueueItem item in queueItems) {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }


                    Log(logger, "Downloading: " + item.Label);

                    bool failed = false;

                    if (item.Type == QobuzQueueItem.ItemType.Album) {
                        int[] album_tracks = qobuzApi.get_album_tracks(item.ItemID);
                        foreach (int id in album_tracks) {
                            int status = songProcessor.process_song(id);
                            if (status != 0) failed = true;
                        }
                    }
                    else {
                        if (songProcessor.process_song(item.ItemID) != 0) failed = true;
                    }

                    if (!failed) config.RemoveItem(item.ItemID);
                    MusicObtainment.Instance!.SaveConfiguration();
                }             
            }
        }, cancellationToken).ConfigureAwait(false);
    }
}