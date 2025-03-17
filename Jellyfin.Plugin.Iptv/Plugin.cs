using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using ICU4N.Logging;
using Jellyfin.Iptv.Service;
using Jellyfin.Plugin.Iptv.Api;
using Jellyfin.Plugin.Iptv.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Jellyfin.Plugin.Iptv;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasPluginConfiguration
{

    //private static Plugin? _instance;
    private readonly ILogger<Plugin> _logger;

    private IListingsManager _listingsManager;
    private MediaBrowser.Common.Configuration.IConfigurationManager _config;


    public Plugin(
        ILogger<Plugin> logger,
        IHttpClientFactory httpClientFactory,
        IServerApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ITaskManager taskManager,
        IListingsManager listingsManager,
        MediaBrowser.Common.Configuration.IConfigurationManager config

        )
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;
        _listingsManager = listingsManager;
        _config = config;

        TaskService = new(taskManager);





        //listingsManager.SaveListingProvider(new IptvListingsProvider(), false, false);

        //configManager.GetLiveTvConfiguration();
        //var liveTvOptions = configManager.GetConfiguration<LiveTvOptions>("livetv");
        //liveTvOptions.ListingProviders;


        /*
                using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
            .AddConsole());
                ILogger exLogger = loggerFactory.CreateLogger<Region>();
                exLogger.LogInformation("Example log message");*/
        // ILogger logger = loggerFactory.CreateLogger<Program>();
        // logger.LogInformation("Example log message");


        logger.LogInformation("Plugin loaded");
    }

    /// <inheritdoc />
    public override string Name => "Iptv";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("4e447dd6-5725-44a5-ac9c-d23e23f6343e");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin Instance { get; private set; }

    //public IptvService IptvService { get; init; }

    public TaskService TaskService { get; init; }

    /// <inheritdoc />
    private static PluginPageInfo CreateStatic(string name) => new()
    {
        Name = name,
        EmbeddedResourcePath = string.Format(
            CultureInfo.InvariantCulture,
            "{0}.Configuration.Web.{1}",
            typeof(Plugin).Namespace,
            name),
    };

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {

            CreateStatic("IptvChannels.html"),
            CreateStatic("IptvChannels.js"),
            CreateStatic("Iptv.js"),
            CreateStatic("Iptv.css"),

        };
    }

    ListingsProviderInfo? info = null;

    public virtual void UpdateConfiguration(BasePluginConfiguration configuration)
    {
        base.UpdateConfiguration(configuration);
        _logger.LogInformation("Updated configuration listings provider: {0}",info?.Id);

        var lto = _config.GetConfiguration<LiveTvOptions>("livetv");
        foreach (var item in lto.ListingProviders)
        {
            _logger.LogError("provider: {0} = {1}", item.Id, item.Type);
        }

        info = lto.ListingProviders
          //.Select(info => info)
          .Where(info => string.Equals("iptv", info.Type, StringComparison.OrdinalIgnoreCase))
          .FirstOrDefault(info);

        _logger.LogInformation("found iptv provider: {0}", info?.Id);

        if (info == null)
        {

            //var config = _config.GetLiveTvConfiguration();




            //.First(); // Already filtered out null





            info = new()
            {
                Id = "iptv",
                EnabledTuners = ["iptv","m3u"],
                Type = "iptv",
                ListingsId = "iptv listings",
                Path = "IPTV Path"
            };
            info = _listingsManager.SaveListingProvider(info, false, false).Result;

            _logger.LogError("saved info with id: {0}", info.Id);


        }
        /*else
        {
            _listingsManager.DeleteListingsProvider("iptv");
            info = null;
            _logger.LogInformation("deleted listings info");
        }
*/


            /* var liveTvOptions = configManager.GetConfiguration<LiveTvOptions>("livetv");
             _logger.LogError("liveTvOptions: {0}", liveTvOptions);

             _logger.LogError("saved info {0}", liveTvOptions.ListingProviders);

             foreach (var p in liveTvOptions.ListingProviders)
             {

                 _logger.LogError("provider: {0}", p);
             }*/

            //public Task<List<NameIdPair>> GetLineups(string? providerType, string? providerId, string? country, string? location)


            List<NameIdPair> r = _listingsManager.GetLineups("iptv", null, null, null).Result;

        _logger.LogError("lineups: {0}", r);

        // Force a refresh of TV guide on configuration update.
        // - This will update the TV channels.
        // - This will remove channels on credentials change.
        TaskService.CancelIfRunningAndQueue(
            "Jellyfin.LiveTv",
            "Jellyfin.LiveTv.Guide.RefreshGuideScheduledTask");

        // Force a refresh of Channels on configuration update.
        // - This will update the channel entries.
        // - This will remove channel entries on credentials change.
        TaskService.CancelIfRunningAndQueue(
            "Jellyfin.LiveTv",
            "Jellyfin.LiveTv.Channels.RefreshChannelsScheduledTask");

    }

}
