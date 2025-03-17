// Copyright (C) 2022  Kevin Jilissen

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Iptv.Api;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Iptv;

/// <summary>
/// Class LiveTvService.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IptvLiveTvService"/> class.
/// </remarks>
/// <param name="appHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
/// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
/// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
/// <param name="memoryCache">Instance of the <see cref="IMemoryCache"/> interface.</param>
public class IptvLiveTvService : ILiveTvService, ISupportsDirectStreamProvider
{

    private readonly ILogger<IptvLiveTvService> _logger;
    private readonly IptvService _iptvService;
    private readonly IMediaSourceManager _mediaSourceManager;
    private readonly INetworkManager _networkManager;
    public IptvLiveTvService(ILogger<IptvLiveTvService> logger, IptvService iptvService, IMediaSourceManager mediaSourceManager, INetworkManager networkManager)
    {
        _iptvService = iptvService;
        _logger = logger;
        _mediaSourceManager = mediaSourceManager;
        _networkManager = networkManager;

    }
    /// <inheritdoc />
    public string Name => "IPTV";

    /// <inheritdoc />
    public string HomePageUrl => string.Empty;

    public string ChannelId => "test-channel-id";
    public string ChannelName => "IPTV Channel";
    public string ChannelNum => "145";

    /// <inheritdoc />
    public async Task<IEnumerable<ChannelInfo>> GetChannelsAsync(CancellationToken cancellationToken)
    {
        _logger.LogError("GetChannelsAsync");
        Plugin plugin = Plugin.Instance;
        List<ChannelInfo> items = [];


        foreach(string channelId in plugin.Configuration.ChannelIds)
        {
            if (_iptvService.GetChannel(channelId, out Channel? channel))
            {
                items.Add(new ChannelInfo()
                {
                    Id = channelId,
                    TunerHostId = "iptv",
                    ChannelType = MediaBrowser.Model.LiveTv.ChannelType.TV,
                    ImageUrl = channel.Logo,
                    HasImage = channel.Logo != null,
                    Tags = channel.Categories.ToArray(),
                    Name = channel.Name
                });
            }
        }

        return items;
    }

    /// <inheritdoc />
    public Task CancelTimerAsync(string timerId, CancellationToken cancellationToken)
    {
        _logger.LogError("CancelTimerAsync");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task CreateTimerAsync(TimerInfo info, CancellationToken cancellationToken)
    {
        _logger.LogError("CreateTimerAsync");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<TimerInfo>> GetTimersAsync(CancellationToken cancellationToken)
    {
        _logger.LogError("GetTimersAsync");
        return Task.FromResult<IEnumerable<TimerInfo>>(new List<TimerInfo>());
    }

    /// <inheritdoc />
    public Task<IEnumerable<SeriesTimerInfo>> GetSeriesTimersAsync(CancellationToken cancellationToken)
    {
        _logger.LogError("GetSeriesTimersAsync");
        return Task.FromResult<IEnumerable<SeriesTimerInfo>>(new List<SeriesTimerInfo>());
    }

    /// <inheritdoc />
    public Task CreateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken)
    {
        _logger.LogError("CreateSeriesTimerAsync");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task UpdateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken)
    {
        _logger.LogError("UpdateSeriesTimerAsync");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task UpdateTimerAsync(TimerInfo updatedTimer, CancellationToken cancellationToken)
    {
        _logger.LogError("UpdateTimerAsync");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task CancelSeriesTimerAsync(string timerId, CancellationToken cancellationToken)
    {
        _logger.LogError("CancelSeriesTimerAsync");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<List<MediaSourceInfo>> GetChannelStreamMediaSources(string channelId, CancellationToken cancellationToken)
    {
        _logger.LogError("GetChannelStreamMediaSources");

        //return Task.FromResult(new List<MediaSourceInfo> { CreateMediaSourceInfo(tuner, channel) });

        //MediaSourceInfo source = await GetChannelStream(channelId, string.Empty, cancellationToken).ConfigureAwait(false);
        //return new List<MediaSourceInfo> { CreateMediaSourceInfo(tuner, channel) };
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<MediaSourceInfo> GetChannelStream(string channelId, string streamId, CancellationToken cancellationToken)
    {
        _logger.LogError("GetChannelStream");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task CloseLiveStream(string id, CancellationToken cancellationToken)
    {
        _logger.LogError("CloseLiveStream {ChannelId}", id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SeriesTimerInfo> GetNewTimerDefaultsAsync(CancellationToken cancellationToken, ProgramInfo? program = null)
    {
        _logger.LogError("GetNewTimerDefaultsAsync");
        return Task.FromResult(new SeriesTimerInfo
        {
            PostPaddingSeconds = 120,
            PrePaddingSeconds = 120,
            RecordAnyChannel = false,
            RecordAnyTime = true,
            RecordNewOnly = false
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
    {
        _logger.LogError("GetProgramsAsync channel: {0}  {1} -> {2}", channelId,startDateUtc, endDateUtc);
        var items = new List<ProgramInfo>();
        items.Add(new()
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
        });

        IEnumerable<ProgramInfo> r = from epg in items
               where epg.EndDate >= startDateUtc && epg.StartDate < endDateUtc
               select epg;
        _logger.LogInformation("returning: {0}", r);
        return r;
        /*Guid guid = Guid.Parse(channelId);
        StreamService.FromGuid(guid, out int prefix, out int streamId, out int _, out int _);
        if (prefix != StreamService.LiveTvPrefix)
        {
            throw new ArgumentException("Unsupported channel");
        }

        string key = $"xtream-epg-{channelId}";
        ICollection<ProgramInfo>? items = null;
        if (memoryCache.TryGetValue(key, out ICollection<ProgramInfo>? o))
        {
            items = o;
        }
        else
        {
            items = new List<ProgramInfo>();
            Plugin plugin = Plugin.Instance;
            using (XtreamClient client = new XtreamClient())
            {
                EpgListings epgs = await client.GetEpgInfoAsync(plugin.Creds, streamId, cancellationToken).ConfigureAwait(false);
                foreach (EpgInfo epg in epgs.Listings)
                {
                    items.Add(new()
                    {
                        Id = StreamService.ToGuid(StreamService.EpgPrefix, streamId, epg.Id, 0).ToString(),
                        ChannelId = channelId,
                        StartDate = epg.Start,
                        EndDate = epg.End,
                        Name = epg.Title,
                        Overview = epg.Description,
                    });
                }
            }

            memoryCache.Set(key, items, DateTimeOffset.Now.AddMinutes(10));
        }

        return from epg in items
               where epg.EndDate >= startDateUtc && epg.StartDate < endDateUtc
               select epg;*/
    }

    /// <inheritdoc />
    public Task ResetTuner(string id, CancellationToken cancellationToken)
    {
        _logger.LogError("ResetTuner");
        throw new NotImplementedException();
    }



    /// <inheritdoc />
    public async Task<ILiveStream> GetChannelStreamWithDirectStreamProvider(string channelId, string streamId, List<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
    {
        _logger.LogError("GetChannelStreamWithDirectStreamProvider: {0}  {1}", channelId, streamId);
        Guid guid = Guid.Parse(channelId);
       /* StreamService.FromGuid(guid, out int prefix, out int channel, out int _, out int _);
        if (prefix != StreamService.LiveTvPrefix)
        {
            throw new ArgumentException("Unsupported channel");
        }

        Plugin plugin = Plugin.Instance;
        MediaSourceInfo mediaSourceInfo = plugin.StreamService.GetMediaSourceInfo(StreamType.Live, channel, restream: true);
        ILiveStream? stream = currentLiveStreams.Find(stream => stream.TunerHostId == Restream.TunerHost && stream.MediaSource.Id == mediaSourceInfo.Id);

        if (stream == null)
        {
            stream = new Restream(appHost, httpClientFactory, logger, mediaSourceInfo);
            await stream.Open(cancellationToken).ConfigureAwait(false);
        }

        stream.ConsumerCount++;*/
        return null;
    }

    protected virtual MediaSourceInfo CreateMediaSourceInfo(TunerHostInfo info, ChannelInfo channel)
    {
        var path = channel.Path;

        var supportsDirectPlay = !info.EnableStreamLooping && info.TunerCount == 0;
        var supportsDirectStream = !info.EnableStreamLooping;

        var protocol = _mediaSourceManager.GetPathProtocol(path);

        var isRemote = true;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            isRemote = !_networkManager.IsInLocalNetwork(uri.Host);
        }

        var httpHeaders = new Dictionary<string, string>();

        if (protocol == MediaProtocol.Http)
        {
            // Use user-defined user-agent. If there isn't one, make it look like a browser.
            httpHeaders[HeaderNames.UserAgent] = string.IsNullOrWhiteSpace(info.UserAgent) ?
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36" :
                info.UserAgent;
        }

        var mediaSource = new MediaSourceInfo
        {
            Path = path,
            Protocol = protocol,
            MediaStreams = new MediaStream[]
            {
                    new MediaStream
                    {
                        Type = MediaStreamType.Video,
                        // Set the index to -1 because we don't know the exact index of the video stream within the container
                        Index = -1,
                        IsInterlaced = true
                    },
                    new MediaStream
                    {
                        Type = MediaStreamType.Audio,
                        // Set the index to -1 because we don't know the exact index of the audio stream within the container
                        Index = -1
                    }
            },
            RequiresOpening = true,
            RequiresClosing = true,
            RequiresLooping = info.EnableStreamLooping,

            ReadAtNativeFramerate = false,

            Id = channel.Path.GetMD5().ToString("N", CultureInfo.InvariantCulture),
            IsInfiniteStream = true,
            IsRemote = isRemote,

            IgnoreDts = info.IgnoreDts,
            SupportsDirectPlay = supportsDirectPlay,
            SupportsDirectStream = supportsDirectStream,

            RequiredHttpHeaders = httpHeaders,
            UseMostCompatibleTranscodingProfile = !info.AllowFmp4TranscodingContainer,
            FallbackMaxStreamingBitrate = info.FallbackMaxStreamingBitrate
        };

        mediaSource.InferTotalBitrate();

        return mediaSource;
    }
}
