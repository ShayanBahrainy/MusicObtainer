using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Music.Configuration;

/// <summary>
/// Represents an item to download from Qobuz
/// </summary>
public class QobuzQueueItem
{
    /// <summary>
    /// Label for the item
    /// </summary>
    public required string Label {get; set;}

    /// <summary>
    /// ID of the item
    /// </summary>
    public int ItemID {get; set;}

    /// <summary>
    /// Enum to represent the item type
    /// </summary>
    public enum ItemType {
        /// <summary>
        /// Item is a Track
        /// </summary>
        Track, 
        /// <summary>
        /// Item is an Album
        /// </summary>
        Album
    };


    /// <summary>
    /// Type of the item
    /// </summary>
    public ItemType Type {get; set;}

}

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{

    /// <summary>
    /// Represents all items in the queue
    /// </summary>
    #pragma warning disable
    public List<QobuzQueueItem> queueItems {get; set;}
    #pragma warning restore
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        queueItems = [];
        
        UserAuth = "SecretUserAuth123";
        AppSecret = "SuperSecretShhh";
        AppID = 798273057;
    }

    /// <summary>
    /// Add item to queue
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(QobuzQueueItem item)
    {
        queueItems.Add(item);
    }

    /// <summary>
    ///  Removes an item from the queue
    /// </summary>
    /// <param name="itemID">int</param>
    public void RemoveItem(int itemID)
    {
        for (int i = queueItems.Count - 1; i > -1; i--)
        {
            if (queueItems[i].ItemID == itemID)
            {
                queueItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Returns the next item on the queue to be downloaded
    /// </summary>
    /// <returns>QobuzQueueItem</returns>

    public QobuzQueueItem? NextItem()
    {
        return queueItems.Count > 0 ? queueItems[0] : null;
    }

    /// <summary>
    /// Gets or sets the UserAuth
    /// </summary>
    public string UserAuth { get; set; }

    /// <summary>
    /// Gets or sets the App Secret
    /// </summary>
    public string AppSecret {get; set; }

    /// <summary>
    /// Gets ot sets the App ID
    /// </summary>
    public int AppID {get; set;}

}
