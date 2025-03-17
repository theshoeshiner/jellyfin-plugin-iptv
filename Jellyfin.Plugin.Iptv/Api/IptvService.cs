using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Iptv.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Iptv.Api
{
    public class IptvService
    {

        private readonly ILogger<IptvService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServerApplicationPaths _serverPaths;

        public IptvService(ILogger<IptvService> logger, IHttpClientFactory httpClientFactory, IServerApplicationPaths serverPaths)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _serverPaths = serverPaths;
        }

        public Dictionary<string, Country> Countries { get; } = [];

        public Dictionary<string, Channel> Channels { get; } = [];

        public Dictionary<string, Language> Languages { get; } = [];

        public Dictionary<string, Region> Regions { get; } = [];

        public Dictionary<string, Category> Categories { get; } = [];

        public Dictionary<string, List<Stream>> Streams { get; } = [];

        public Dictionary<string, List<Guide>> Guides { get; } = [];

        /*   public bool SetStream(string channelId, Stream stream)
           {
               if (Channels.TryGetValue(channelId, out var channel) && Streams.TryAdd(channelId, stream))
               {
                   channel.HasStream = true;
                   return true;
               }
               return false;
           }*/

        public bool AddStream(string channelId, Stream stream)
        {
            if (Channels.TryGetValue(channelId, out var channel))
            {
                if (!Streams.TryGetValue(channelId, out var list))
                {
                    list = [];
                    Streams.Add(channelId, list);
                }
                list.Add(stream);
                channel.HasStream = true;
                return true;
            }
            return false;
        }

        public bool AddGuide(string channelId, Guide guide)
        {
            if (Channels.TryGetValue(channelId, out var channel))
            {
                if (!Guides.TryGetValue(channelId, out var guideList))
                {
                    guideList = [];
                    Guides.Add(channelId, guideList);
                }
                guideList.Add(guide);
                channel.HasGuide = true;
                return true;
            }
            return false;
        }

  /*      public Channel? GetChannel(string id)
        {
            Channels.TryGetValue(id, out var channel);
            return channel;
        }*/

        public bool GetFirstStream(string channelId, out Stream? stream)
        {
            if (Streams.TryGetValue(channelId, out var streams))
            {
                stream = streams[0];
                return true;
            }
            stream = null;
            return false;
        }

        public bool GetChannel(string id, out Channel channel)
        {
            return Channels.TryGetValue(id, out channel);
        }

        public Collection<Channel> GetFilteredChannels(ChannelFilter filter)
        {
            Collection<Channel> channels = [];
            foreach (Channel channel in Channels.Values)
            {
                if (channel.Languages.Intersect(filter.Languages).Any())
                {
                    if (channel.Categories.Intersect(filter.Categories).Any())
                    {
                        if (filter.Countries.Contains(channel.Country.ToLowerInvariant()))
                        {
                            _logger.LogDebug("Found channel matching filter: {0}", channel);
                            channels.Add(channel);
                        }
                    }
                }
            }

            _logger.LogInformation("{0} matching channels", channels.Count);
            return channels;
        }

        public Collection<Channel> GetAllChannels()
        {
            _logger.LogInformation("GetAllChannels {0}", Channels.Values.Count);
            return [.. Channels.Values];
        }

    }
}
