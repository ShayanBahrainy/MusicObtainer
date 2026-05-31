using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Jellyfin.Plugin.Music.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

using Python.Runtime;

namespace Jellyfin.Plugin.MusicObtainer;

/// <summary>
/// The main plugin.
/// </summary>
public class MusicObtainment : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MusicObtainer"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface. </param> 
    ///  <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface. </param> 
    public MusicObtainment(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager, ILogger<MusicObtainment> logger)
        : base(applicationPaths, xmlSerializer)
    {
        this.LibraryManager = libraryManager;
        List<string> dllLocations = [];
        dllLocations.AddRange(Directory.GetFiles("/usr/lib64", "libpython3.*.so*", SearchOption.TopDirectoryOnly));
        dllLocations.AddRange(Directory.GetFiles("/usr/lib", "libpython3.*.so*", SearchOption.TopDirectoryOnly));

        if (dllLocations.Count == 0) throw new FileNotFoundException("Python runtime library not found");

        Runtime.PythonDLL = dllLocations[0];

        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();
        Instance = this;
    }

    /// <summary>
    /// Library manager.
    /// </summary>
    public ILibraryManager LibraryManager {get; set;}

    /// <inheritdoc />
    public override string Name => "Music Obtainer";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("5c24b130-bbc3-4c69-aedb-e180018792d9");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static MusicObtainment? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                DisplayName = "Qobuz Queue",
                EnableInMainMenu = true,
                MenuSection = "Server",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}
