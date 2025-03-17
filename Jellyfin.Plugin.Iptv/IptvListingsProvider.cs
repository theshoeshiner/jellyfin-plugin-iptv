using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Iptv.Api;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Iptv
{
    public class IptvListingsProvider : IListingsProvider
    {

        private readonly ILogger<IptvListingsProvider> _logger;
        private readonly IptvService _iptvService;
        private readonly IMediaSourceManager _mediaSourceManager;
        private readonly INetworkManager _networkManager;

        public IptvListingsProvider(
                ILogger<IptvListingsProvider> logger,
                IptvService iptvService,
                IMediaSourceManager mediaSourceManager,
                INetworkManager networkManager
              )
        {
            _logger = logger;
            _mediaSourceManager = mediaSourceManager;
            _networkManager = networkManager;
            _iptvService = iptvService;
            logger.LogError("created IptvListingsProvider");
            //_listingsManager.SaveListingProvider(this, false, false);
        }

        public string Name => "IPTV Listings";

        public string Type => "iptv";

        public Task<List<ChannelInfo>> GetChannels(ListingsProviderInfo info, CancellationToken cancellationToken)
        {
            _logger.LogError("GetChannels info: {0}", info);


            Plugin plugin = Plugin.Instance;
            List<ChannelInfo> items = [];

            foreach (string channelId in plugin.Configuration.ChannelIds)
            {
                //var channel = _iptvService.GetChannel(channelId);
                _iptvService.GetChannel(channelId, out var channel);
                if (channel != null)
                {
                    items.Add(new ChannelInfo()
                    {
                        Id = channelId,
                        TunerHostId = "iptv",
                        ChannelType = ChannelType.TV,
                        ImageUrl = channel.Logo,
                        HasImage = channel.Logo != null,
                        Tags = [.. channel.Categories],
                        Name = channel.Name,
                    });
                }
            }

            _logger.LogError("returning {0} channels", items.Count);

            return Task.FromResult<List<ChannelInfo>>(items);

        }

        public Task<List<NameIdPair>> GetLineups(ListingsProviderInfo info, string country, string location)
        {
            _logger.LogError("GetLineups info: {0}", info);
            return Task.FromResult(new List<NameIdPair>());
        }

        public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(ListingsProviderInfo info, string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
        {
            _logger.LogError("GetProgramsAsync info: {0}", info);
            _logger.LogError("GetProgramsAsync channel: {0}  {1} -> {2}", channelId, startDateUtc, endDateUtc);
            //public DateTime(int year, int month, int day, int hour, int minute, int second);
            DateTime currentHour = new DateTime(startDateUtc.Year, startDateUtc.Month, startDateUtc.Day, startDateUtc.Hour, 0, 0);
            DateTime endHour = new DateTime(endDateUtc.Year, endDateUtc.Month, endDateUtc.Day, endDateUtc.Hour, 0, 0);
            var items = new List<ProgramInfo>();
            _iptvService.GetChannel(channelId, out Channel channel);

            while (currentHour.CompareTo(endHour) < 0)
            {
                DateTime nextTime = currentHour.AddHours(1);
                string placeholderId = string.Empty + currentHour.Year + currentHour.Month + currentHour.Day + currentHour.Hour;
                DateTime localStart = currentHour.ToLocalTime();
                DateTime localEnd = localStart.AddHours(1);
                var startString = localStart.ToString("htt").ToLowerInvariant();
                var endString = localEnd.ToString("htt").ToLowerInvariant();
                var name = startString + " to " + endString + " on " + channel.Name;
                items.Add(new()
                {
                    Id = "hourly-placeholder-" + placeholderId + "channelId",
                    ChannelId = channelId,
                    StartDate = currentHour,
                    EndDate = nextTime,
                    Name = name,
                    Overview = "Placeholder for timeslot " + name,
                });
                currentHour = nextTime;
            }

            /* items.Add(new()
             {
                 Id = "test-program-id",
                 ChannelId = channelId,
                 StartDate = DateTime.Now.AddMinutes(-30).ToUniversalTime(),
                 EndDate = DateTime.Now.AddMinutes(60).ToUniversalTime(),
                 Name = "Test Program for Channel " + channelId,
                 Overview = "Some Description for this program",
             });

             items.Add(new()
             {
                 Id = "test-program-id-2",
                 ChannelId = channelId,
                 StartDate = DateTime.Now.AddMinutes(60).ToUniversalTime(),
                 EndDate = DateTime.Now.AddMinutes(175).ToUniversalTime(),
                 Name = "Test Future Program for Channel " + channelId,
                 Overview = "Some Description for this program",
             });*/

            /*  IEnumerable<ProgramInfo> r = from epg in items
                                           where epg.EndDate >= startDateUtc && epg.StartDate < endDateUtc
                                           select epg;*/
            _logger.LogInformation("returning: {0} items", items.Count);
            return items;

            //throw new NotImplementedException();
        }

        public Task Validate(ListingsProviderInfo info, bool validateLogin, bool validateListings)
        {
            _logger.LogError("Validate info: {0}", info);
            return Task.CompletedTask;
        }
    }
}
