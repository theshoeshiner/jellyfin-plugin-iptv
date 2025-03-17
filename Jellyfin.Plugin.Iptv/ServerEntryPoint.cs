using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICU4N.Logging;
using Jellyfin.Plugin.Iptv.Api;
using Jellyfin.Plugin.Iptv.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Iptv
{
    internal class ServerEntryPoint(
        ILogger<ServerEntryPoint> logger,
        IHttpClientFactory httpClientFactory,
        IServerApplicationPaths serverPaths,
        IEnumerable<IListingsProvider> listingsProviders,
        IListingsManager listingsManager,
        IConfigurationManager configManager,
        ITunerHostManager tunerHostManager) : IHostedService
    {

        //private IServerConfigurationManager? _configurationManager;
        //private IMetadataSaver? _metadataSaver;
        //private ILogger<ServerEntryPoint> _logger;
        //private ILibraryManager manager;
        //private ISubtitleManager _subtitleManager;
        //private ITaskManager taskManager;


        public Task StartAsync(CancellationToken cancellationToken)
        {
            //serverPaths.CachePath
            logger.LogInformation("StartAsync");








            foreach (IListingsProvider p in listingsProviders)
            {
                logger.LogError("IListingsProvider type: {0}",p.Type);
            }

            foreach (ITunerHost p in tunerHostManager.TunerHosts)
            {
                logger.LogError("ITunerHost: {0}", p.Name);
            }
            // public async Task<ListingsProviderInfo> SaveListingProvider(ListingsProviderInfo info, bool validateLogin, bool validateListings)
            //{
/*            ListingsProviderInfo info = new() {
                Id = "iptv",
                EnabledTuners = ["iptv"]

            };

            listingsManager.SaveListingProvider(info,false,false);*/

                loadChannels(cancellationToken);

           // httpClientFactory.CreateClient(NamedClient.Default).GetAsync(channelsJsonUrl)

           /* HttpResponseMessage response = httpClientFactory.CreateClient(NamedClient.Default)
            .GetAsync(channelsJsonUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(true).*/
/**/
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public async void loadChannels(CancellationToken cancellationToken)
        {
            string channelsUrl = "https://iptv-org.github.io/api/channels.json";
            string countriesUrl = "https://iptv-org.github.io/api/countries.json";

            HttpClient client = httpClientFactory.CreateClient(NamedClient.Default);

            logger.LogInformation("Getting channels");

            List<Country> countries = await httpClientFactory.CreateClient(NamedClient.Default)
          .GetFromJsonAsync<List<Country>>(countriesUrl, cancellationToken).ConfigureAwait(true);

            logger.LogInformation("got {0} countries", countries != null ? countries.Count : null);

            List<Channel> channels = await httpClientFactory.CreateClient(NamedClient.Default)
           .GetFromJsonAsync<List<Channel>>(channelsUrl, cancellationToken).ConfigureAwait(true);


            logger.LogInformation("got {0} channels", channels != null ? channels.Count : null);

            /* List<Channel> channels = await httpClientFactory.CreateClient(NamedClient.Default)
             .GetFromJsonAsync(channelsJsonUrl,null,null, cancellationToken)
             .ConfigureAwait(true);*/

            // httpClientFactory.CreateClient(NamedClient.Default).GetFromJsonAsync

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StopAsync");
            // throw new NotImplementedException();
            return Task.CompletedTask;
        }
        /*
       private void OnItemAdded(object? sender, ItemChangeEventArgs e)
       {

           _logger.LogWarning("OnItemAdded sender: {0} item: {1}", sender, e.Item);
           var item = e.Item;
           checkStreams(e.Item);

       }

       private void OnItemUpdated(object? sender, ItemChangeEventArgs e)
       {

           _logger.LogWarning("OnItemUpdated sender: {0} item: {1}", sender, e.Item);
           checkStreams(e.Item);
       }

       private void OnItemRemoved(object? sender, ItemChangeEventArgs e)
       {

           _logger.LogWarning("OnItemRemoved sender: {0} item: {1}", sender, e.Item);
       }

       private void checkStreams(BaseItem item)
       {
           _logger.LogInformation("checkStreams: {0}", item.GetType());
           if (item.GetType() == typeof(Movie))
           {
               //_logger.LogInformation("was movie");
               var movie = (Movie)item;


               //MediaBrowser.Controller.Entities.Movies.Movie movie = (MediaBrowser.Controller.Entities.Movies.Movie)item;

               //_logger.LogInformation("checkStreams: {0}", movie.GetMediaStreams());

               foreach (var s in movie.GetMediaStreams())
               {
                   // _logger.LogWarning("stream type: {0} = {1}", s.Type, s);

                   if (s.Type == MediaStreamType.Subtitle)
                   {
                       if (s.IsInterlaced)
                       {

                       }
                       else
                       {
                           _logger.LogWarning("subtitle stream needs to be syncd: {0}", s);
                           var pc = Plugin.Instance!.Configuration;

                           taskManager.QueueIfNotRunning<ScheduledTask>();

                           _logger.LogWarning("queued task");

                       }
                   }
               }

           }
       }
       public void Dispose()
       {
           Console.Out.WriteLine("Dispose");

       }

       public Task RunAsync()
       {
           Console.Out.WriteLine("RunAsync");

           return Task.CompletedTask;
       }*/
    }
}
