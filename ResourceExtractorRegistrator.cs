using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.MusicObtainer;

/// <summary>
/// Registers the resource extractor
/// </summary>
public class ResourceExtracterRegistrator : IPluginServiceRegistrator
{

    /// <summary>
    /// Registers the resource extractor
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="applicationHost"></param>
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IHostedService, ResourceExtracter>();
    }
    
}