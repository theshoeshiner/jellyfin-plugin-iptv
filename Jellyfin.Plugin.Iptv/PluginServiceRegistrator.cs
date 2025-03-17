using System;
using Jellyfin.Plugin.Iptv.Api;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Iptv;

/// <inheritdoc />
public class PluginServiceRegistrator() : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        Console.Out.WriteLine("RegisterServices");
        serviceCollection.AddSingleton<IptvService, IptvService>();
        serviceCollection.AddSingleton<ITunerHost, IptvTunerHost>();

        serviceCollection.AddSingleton<IListingsProvider, IptvListingsProvider>();

        serviceCollection.AddSingleton<IHostedService, IptvLoader>();
        //serviceCollection.AddSingleton<ILiveTvService, IptvLiveTvService>();





        //serviceCollection.AddSingleton<IHostedService, ServerEntryPoint>();

        //serviceCollection.AddSingleton<IChannel, CatchupChannel>();
        //serviceCollection.AddSingleton<IChannel, SeriesChannel>();
        //serviceCollection.AddSingleton<IChannel, VodChannel>();
        //serviceCollection.AddSingleton<IPreRefreshProvider, XtreamVodProvider>();
        Console.Out.WriteLine("Done Registering Services");

    }
}
