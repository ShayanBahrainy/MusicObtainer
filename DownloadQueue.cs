
using System.Collections.Generic;
using MediaBrowser.Model.Tasks;
using System.Threading.Tasks;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;


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

    /// <summary>
    /// Executes task
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        LogTaskRun(logger, LogLevel.Information);
        await Task.Run(() => {}, CancellationToken.None).ConfigureAwait(false);
    }



    
}