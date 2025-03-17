using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ICU4N.Logging;
using Jellyfin.Plugin.Iptv.Api;
using MediaBrowser.Controller.Dto;
using System.Diagnostics;
using Jellyfin.Extensions;
using System.IO.Pipes;

namespace Jellyfin.Plugin.Iptv
{
    public class IptvTunerHost : ITunerHost, IConfigurableTunerHost
    {

        private static readonly string[] _mimeTypesCanShareHttpStream = ["video/MP2T"];
        private static readonly string[] _extensionsCanShareHttpStream = [".ts", ".tsv", ".m2t"];

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServerApplicationHost _appHost;
        private readonly INetworkManager _networkManager;
        private readonly IMediaSourceManager _mediaSourceManager;
        private readonly IStreamHelper _streamHelper;
        private readonly IptvService _iptvService;

        //private readonly TunerHostInfo _host;

        public IptvTunerHost(
            IServerConfigurationManager config,
            IMediaSourceManager mediaSourceManager,
            ILogger<IptvTunerHost> logger,
            IFileSystem fileSystem,
            IHttpClientFactory httpClientFactory,
            IServerApplicationHost appHost,
            INetworkManager networkManager,
            IStreamHelper streamHelper,
            IptvService iptvService)

        {
            _httpClientFactory = httpClientFactory;
            _appHost = appHost;
            _networkManager = networkManager;
            _mediaSourceManager = mediaSourceManager;
            _streamHelper = streamHelper;
            Logger = logger;
            Config = config;
            _iptvService = iptvService;
            FileSystem = fileSystem;
            HostInfo = new()
            {
                Id = "iptv",
                FriendlyName = "IPTV Tuner",
                Type = "iptv",
                DeviceId = "iptv"
                //EnableStreamLooping = true
            };

            /*_host = new()
            {
                Id = "iptv",
                FriendlyName = "IPTV Tuner",
                Type = "iptv",
                DeviceId = "iptv"
            };*/
            logger.LogInformation("instantiated IptvTunerHost");
        }

        protected ILogger<IptvTunerHost> Logger { get; }

        protected IFileSystem FileSystem { get; }

        protected IServerConfigurationManager Config { get; }

        public string Type => "iptv";

        public string Name => "IPTV Tuner";

        public bool IsSupported => true;

        public TunerHostInfo HostInfo { get; set; }


        public async Task Validate(TunerHostInfo info)
        {
            // force this id so that we never create more than one iptv tuner
            info.Id = "iptv";
        }

        public Task<List<TunerHostInfo>> DiscoverDevices(int discoveryDurationMs, CancellationToken cancellationToken)
        {
            return Task.FromResult<List<TunerHostInfo>>([HostInfo]);
        }

        protected ChannelInfo? GetChannel(string channelId, bool enableCache, CancellationToken cancellationToken)
        {
            if (_iptvService.GetChannel(channelId, out Channel channel))
            {
                _iptvService.GetFirstStream(channelId, out Api.Stream? stream);

                return new ChannelInfo()
                {
                    Id = channelId,
                    TunerHostId = "iptv",
                    ChannelType = ChannelType.TV,
                    ImageUrl = channel.Logo,
                    HasImage = channel.Logo is not null,
                    Tags = [.. channel.Categories],
                    Name = channel.Name,
                    Path = stream?.Url
                };
            }
            return null;
        }

        public Task<List<ChannelInfo>> GetChannels(bool enableCache, CancellationToken cancellationToken)
        {
            Plugin plugin = Plugin.Instance;
            List<ChannelInfo> items = [];
            foreach (string channelId in plugin.Configuration.ChannelIds)
            {
                ChannelInfo? info = GetChannel(channelId, enableCache, cancellationToken);
                if (info is not null)
                {
                    items.Add(info);
                }
            }
            Logger.LogDebug("returning {0} channels", items.Count);
            return Task.FromResult<List<ChannelInfo>>(items);
        }



        public async Task<ILiveStream> GetChannelStream(string channelId, string streamId, IList<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(channelId);
            ChannelInfo? channelInfo = GetChannel(channelId, true, cancellationToken);

            if (channelInfo != null)
            {
                var liveStream = await GetChannelStream(HostInfo, channelInfo, streamId, currentLiveStreams, cancellationToken).ConfigureAwait(false);
                var startTime = DateTime.UtcNow;
                await liveStream.Open(cancellationToken).ConfigureAwait(false);
                var endTime = DateTime.UtcNow;
                // Logger.LogInformation("Live stream opened after {0}ms", (endTime - startTime).TotalMilliseconds);
                return liveStream;

            }
            else throw new LiveTvConflictException("IPTV Channel Not Found");
        }

        protected async Task<ILiveStream> GetChannelStream(TunerHostInfo tunerHost, ChannelInfo channel, string streamId, IList<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
        {
            var tunerCount = tunerHost.TunerCount;

            if (tunerCount > 0)
            {
                var tunerHostId = tunerHost.Id;
                var liveStreams = currentLiveStreams.Where(i => string.Equals(i.TunerHostId, tunerHostId, StringComparison.OrdinalIgnoreCase));

                if (liveStreams.Count() >= tunerCount)
                {
                    throw new LiveTvConflictException("M3U simultaneous stream limit has been reached.");
                }
            }

            var mediaSource = (await GetChannelStreamMediaSources(channel.Id, cancellationToken).ConfigureAwait(false))[0];

            if (tunerHost.AllowStreamSharing && mediaSource.Protocol == MediaProtocol.Http && !mediaSource.RequiresLooping)
            {
                var extension = Path.GetExtension(new UriBuilder(mediaSource.Path).Path);

                if (string.IsNullOrEmpty(extension))
                {
                    try
                    {
                        using var message = new HttpRequestMessage(HttpMethod.Head, mediaSource.Path);
                        using var response = await _httpClientFactory.CreateClient(NamedClient.Default)
                            .SendAsync(message, cancellationToken)
                            .ConfigureAwait(false);

                        if (response.IsSuccessStatusCode)
                        {
                            if (_mimeTypesCanShareHttpStream.Contains(response.Content.Headers.ContentType?.MediaType, StringComparison.OrdinalIgnoreCase))
                            {
                                return new SharedHttpStream(mediaSource, tunerHost, streamId, FileSystem, _httpClientFactory, Logger, Config, _appHost, _streamHelper);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogWarning("HEAD request to check MIME type failed, shared stream disabled");
                    }
                }
                else if (_extensionsCanShareHttpStream.Contains(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return new SharedHttpStream(mediaSource, tunerHost, streamId, FileSystem, _httpClientFactory, Logger, Config, _appHost, _streamHelper);
                }
            }

            return new LiveStream(mediaSource, tunerHost, FileSystem, Logger, Config, _streamHelper);
        }


        public Task<List<MediaSourceInfo>> GetChannelStreamMediaSources(string channelId, CancellationToken cancellationToken)
        {

            ArgumentException.ThrowIfNullOrEmpty(channelId);

            List<MediaSourceInfo> list = [];
            var channelInfo = GetChannel(channelId, true, cancellationToken);

            if (channelInfo is not null && channelInfo.Path is not null)
            {
                list.Add(CreateMediaSourceInfo(channelInfo));
            }

            return Task.FromResult(list);
        }


        protected MediaSourceInfo CreateMediaSourceInfo(ChannelInfo channel)
        {

            var info = HostInfo;
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
}
