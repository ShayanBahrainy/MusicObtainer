using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace Jellyfin.Plugin.MusicObtainer;
/// <summary>
/// Extracts scripts from DLL on startup
/// </summary>
public class ResourceExtracter : IHostedService
{

    /// <summary>
    /// Extracts scripts
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (directory == null) return;

        string[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();


        foreach (string resource in resources)
        {
            if (resource.Contains(".py", System.StringComparison.OrdinalIgnoreCase)) {
                using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

                string[] resourceParts = resource.Split('.');

                string resourceRawName = resourceParts[^2] + '.' + resourceParts[^1];

                using var file = new FileStream(Path.Combine(directory, resourceRawName), FileMode.Create, FileAccess.Write);
                {
                    if (resourceStream != null)
                        await resourceStream.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            
        }, cancellationToken);
    }
    
}