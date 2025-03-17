using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ICU4N.Logging;
using Jellyfin.Extensions;
using Jellyfin.LiveTv.TunerHosts;
using Jellyfin.Plugin.Iptv.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.LiveTv;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Iptv.Api
{


    public class IptvLoader(ILogger<IptvLoader> logger, IHttpClientFactory httpClientFactory, IServerApplicationPaths serverPaths, ITunerHostManager tunerHostManager, IptvService iptvService) : IHostedService
    {

        private const string UrlPrefix = "https://iptv-org.github.io/";
        private const string CacheFolder = "iptv";
        private const string ChannelsFile = "api/channels.json";
        private const string CountriesFile = "api/countries.json";
        private const string LanguagesFile = "api/languages.json";
        private const string StreamsFile = "api/streams.json";
        private const string CategoriesFile = "api/categories.json";
        private const string GuidesFile = "api/guides.json";
        private const string RegionsFile = "api/regions.json";
        private const string PlaylistsFile = "iptv/index.m3u";

        private static readonly Regex _extInfRegex = new Regex("(#EXTINF:((-?[0-9]*)?( (([^=]*?)=\"([^\"]*)\")*)?,(([^\\(]*)(\\((.*)\\))?( \\[(.*)\\])?.*)))");
        private static readonly Regex _nameRegex = new Regex("(.*?)( \\((.*)\\))?( (\\[(.*)\\]))?$");
        private static readonly Collection<string> _files = [ChannelsFile, CountriesFile, LanguagesFile, StreamsFile, CategoriesFile, GuidesFile, RegionsFile, PlaylistsFile];
        private static readonly JsonSerializerOptions _options = new()
        {

            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StartAsync");

            Directory.CreateDirectory(serverPaths.CachePath + "\\" + CacheFolder);

            using (System.IO.Stream output = File.Create(MakeCachePath("epg.exe")))
            {
                logger.LogInformation("output: {0}", output);
                System.IO.Stream em = Assembly.GetExecutingAssembly().GetManifestResourceStream("Jellyfin.Plugin.Iptv.Embedded.epg.exe");
                logger.LogInformation("embedded: {0}", em);
                em.CopyTo(output);
            }

            await DownloadCache(cancellationToken).ConfigureAwait(true);

            _ = LoadEntities<Country>(CountriesFile, iptvService.Countries);
            _ = LoadEntities<Language>(LanguagesFile, iptvService.Languages);
            _ = LoadEntities<Region>(RegionsFile, iptvService.Regions);
            _ = LoadEntities<Category>(CategoriesFile, iptvService.Categories);

            await LoadEntities<Channel>(ChannelsFile, iptvService.Channels).ConfigureAwait(true);

            // Need to load all channels before we load streams and guides
            _ = LoadStreamPlaylist(cancellationToken);
            _ = LoadGuides();

        }

        private void PrintState()
        {

            /*       int max = 0;
                   foreach (KeyValuePair<string, List<Stream>> kvp in iptvService.Streams)
                   {
                       if (kvp.Value.Count >= max)
                       {
                           logger.LogInformation("channel {0} has {1} streams", kvp.Key, kvp.Value.Count);
                           max = kvp.Value.Count;
                       }
                   }
       */
        }

        private async Task LoadStreamPlaylist(CancellationToken cancellationToken)
        {
            logger.LogInformation("LoadPlaylists");

            try
            {
                logger.LogInformation("{0} tuner hosts", tunerHostManager.TunerHosts.Count);
                foreach (ITunerHost host in tunerHostManager.TunerHosts)
                {
                    logger.LogInformation("tuner: {0} = {1}", host.Type,host.Name);
                }

                IptvTunerHost iptvTunerHost = (IptvTunerHost)tunerHostManager.TunerHosts.Where(host => host.Type.Equals("iptv", StringComparison.Ordinal)).First();
                string playlistFile = MakeCachePath(PlaylistsFile);
                logger.LogInformation("url: {0}", playlistFile);
                iptvTunerHost.HostInfo.Url = playlistFile;
                logger.LogInformation("url: {0}", iptvTunerHost.HostInfo.Url);

                List<ChannelInfo> channels = await new M3uParser(NoopLogger.Instance, null)
                   .Parse(iptvTunerHost.HostInfo, string.Empty, cancellationToken)
                   .ConfigureAwait(false);

                logger.LogInformation("parsed {0} channels from m3u using parser", channels.Count);

                // Turn channel infos into streams
                foreach (ChannelInfo channel in channels)
                {

                    if (string.IsNullOrEmpty(channel.TunerChannelId))
                    {
                        continue;
                    }

                    var match = _nameRegex.Matches(channel.Name)[0];
                    var quality = match.Groups[3].Value.Trim();
                    var tags = match.Groups[6].Value.Trim();

                    Stream stream = new Stream()
                    {
                        Channel = channel.TunerChannelId,
                        Url = channel.Path,
                        Quality = quality,
                        Tags = tags
                    };

                    iptvService.AddStream(stream.Channel, stream);
                }

            }
            catch(Exception e)
            {
                logger.LogError(e, "error");
            }

            /*try
            {
                using (var reader = new StreamReader(new FileStream(MakeCachePath(PlaylistsFile), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None)))
                {
                    string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(true);
                    if (line != null && line.StartsWith("#EXTM3U", StringComparison.OrdinalIgnoreCase))
                    {
                        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(true)) != null)
                        {
                            if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                            {
                                Stream? stream = await LoadPlaylistEntry(reader, line).ConfigureAwait(true);
                                if (stream != null)
                                {
                                    iptvService.AddStream(stream.Channel, stream);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "playlist stream error");
            }*/

            logger.LogInformation("Loaded {0} streams", iptvService.Streams.Count);
        }

        private async Task<Stream?> LoadPlaylistEntry(StreamReader reader, string infLine)
        {
            try
            {
                var match = _extInfRegex.Matches(infLine)[0];
                var props = match.Groups[6].Captures;
                var values = match.Groups[7].Captures;

                string? id = null;
                for (int i = 0; i < props.Count; i++)
                {
                    var prop = props[i].Value.Trim();
                    var value = values[i].Value;
                    if (prop.Equals("tvg-id", StringComparison.OrdinalIgnoreCase))
                    {
                        id = value;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                string? line;
                string? url = null;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(true)) != null)
                {
                    if (!line.StartsWith('#'))
                    {
                        url = line;
                        break;
                    }
                }

                if (url == null) return null;

                var name = match.Groups[9].Value.Trim();
                var quality = match.Groups[11].Value.Trim();
                var tags = match.Groups[13].Value.Trim();

                return new Stream()
                {
                    Channel = id,
                    Url = url,
                    Quality = quality,
                    Tags = tags
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "stream error");

            }
            return null;
        }

        private async Task LoadEntities<T>(string file, Dictionary<string, T> map)
            where T : IIptvEntity
        {
            logger.LogInformation("Loading File: {0}", file);
            try
            {
                List<T>? entities = await Deserialize<T>(file).ConfigureAwait(true);
                if (entities != null)
                {
                    foreach (T entity in entities)
                    {
                        string key = entity.Key;
                        if (key != null)
                        {
                            map.Add(key, entity);
                        }
                    }
                }
                logger.LogInformation("Loaded {0} {1} entities", map.Count, typeof(T));
            }
            catch (Exception e)
            {
                logger.LogError(e, "stream error");
            }
        }

        private async Task<List<T>?> Deserialize<T>(string file)
            where T : IIptvEntity
        {
            logger.LogInformation("Loading File: {0}", file);
            using var stream = new FileStream(MakeCachePath(file), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            List<T>? entities = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options).ConfigureAwait(true);
            logger.LogInformation("Deserialized {0} {1} entities", entities?.Count, typeof(T));
            return entities;
        }

        private async Task LoadEntitiesLists<T>(string file, Dictionary<string, List<T>> map)
            where T : IIptvEntity
        {
            logger.LogInformation("Loading File: {0}", file);
            try
            {
                using var stream = new FileStream(MakeCachePath(file), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                List<T>? entities = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options).ConfigureAwait(true);
                if (entities != null)
                {
                    foreach (T entity in entities)
                    {
                        string key = entity.Key;
                        if (key != null)
                        {
                            if (!map.TryGetValue(key, out var value))
                            {
                                value = new List<T>();
                                map.Add(key, value);
                            }

                            value.Add(entity);
                        }
                    }
                }
                logger.LogInformation("Loaded {0} {1} entities", map.Count, typeof(T));
            }
            catch (Exception e)
            {
                logger.LogError(e, "stream error for file {0}", file);
            }
        }


        private async Task LoadGuides()
        {
            List<Guide>? guides = await Deserialize<Guide>(GuidesFile).ConfigureAwait(true);
            if (guides != null)
            {
                foreach (var guide in guides)
                {
                    if (guide.Channel != null)
                    {
                        iptvService.AddGuide(guide.Channel, guide);
                    }

                }
            }
        }



        private async Task DownloadCache(CancellationToken cancellationToken)
        {
            logger.LogInformation("DownloadCache");

            try
            {

                List<Task> tasks = new List<Task>();
                foreach (string file in _files)
                {
                    var task = DownloadFileToCache(file, cancellationToken);
                    tasks.Add(task);
                }

                foreach (Task task in tasks)
                {
                    await task.ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not download files to cache");
            }

        }

        private async Task DownloadFileToCache(string file, CancellationToken cancellationToken)
        {
            logger.LogInformation("DownloadFileToCache {0}", file);
            System.IO.Stream countriesStream = await httpClientFactory.CreateClient(NamedClient.Default).GetStreamAsync(MakeApiPath(file), cancellationToken).ConfigureAwait(true);
            using (var cacheStream = new FileStream(MakeCachePath(file), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                await countriesStream.CopyToAsync(cacheStream, cancellationToken).ConfigureAwait(true);
            }

        }

        private string MakeCachePath(string file)
        {
            return serverPaths.CachePath + "\\" + CacheFolder + "\\" + Path.GetFileName(file);
        }

        private string MakeApiPath(string file)
        {
            return UrlPrefix + file;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
