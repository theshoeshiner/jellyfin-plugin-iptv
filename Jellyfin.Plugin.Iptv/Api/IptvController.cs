using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Iptv.Api;
using Jellyfin.Plugin.Iptv.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Iptv.Api
{
    [ApiController]
    [Route("[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class IptvController(ILogger<IptvController> logger, IptvService iptvService) : ControllerBase
    {

       /* public IptvController(ILogger<IptvService> logger) : this(logger, Plugin.Instance.IptvService)
        {
        }*/

        /*    private static Channel CreateCategoryResponse(Category category) =>
         new()
         {
             Id = category.CategoryId,
             Name = category.CategoryName,
         };*/

        /*[Authorize(Policy = "RequiresElevation")]
        [HttpGet("FilteredChannels")]
        public async Task<ActionResult<IEnumerable<Channel>>> GetFilteredChannels(CancellationToken cancellationToken)
        {


            Plugin plugin = Plugin.Instance;
            PluginConfiguration config = plugin.Configuration;
            ChannelFilter filter = config.ChannelFilter;

            logger.LogInformation("GetFilteredChannels filter: {0}", filter);

            Collection<Channel> channels = iptvService.GetFilteredChannels(filter);
            logger.LogInformation("Got {0} channels", channels.Count);

            return Ok(channels);
        }*/

        [Authorize(Policy = "RequiresElevation")]
        [HttpGet("AllChannels")]
        public async Task<ActionResult<IEnumerable<Channel>>> GetAllChannels(CancellationToken cancellationToken)
        {
            logger.LogInformation("GetAllChannels");
            Collection<Channel> channels = iptvService.GetAllChannels();
            return Ok(channels);
        }

        [Authorize(Policy = "RequiresElevation")]
        [HttpGet("SelectedChannels")]
        public async Task<ActionResult<IEnumerable<string>>> GetSelectedChannelIds(CancellationToken cancellationToken)
        {
            Plugin plugin = Plugin.Instance;
            PluginConfiguration config = plugin.Configuration;
            return Ok(config.ChannelIds);
        }

        [Authorize(Policy = "RequiresElevation")]
        [HttpGet("SelectChannel/{channelId}")]
        public async Task<ActionResult<IEnumerable<string>>> SelectChannel(string channelId, CancellationToken cancellationToken)
        {
            logger.LogInformation("SelectChannel {0}",channelId);
            Plugin plugin = Plugin.Instance;
            PluginConfiguration config = plugin.Configuration;
            config.ChannelIds.Add(channelId);
            plugin.UpdateConfiguration(config);
            return Ok(true);
        }

        [Authorize(Policy = "RequiresElevation")]
        [HttpGet("UnselectChannel/{channelId}")]
        public async Task<ActionResult<IEnumerable<string>>> UnselectChannel(string channelId, CancellationToken cancellationToken)
        {
            logger.LogInformation("UnselectChannel {0}", channelId);
            Plugin plugin = Plugin.Instance;
            PluginConfiguration config = plugin.Configuration;
            config.ChannelIds.Remove(channelId);
            plugin.UpdateConfiguration(config);
            return Ok(true);
        }

    }
}
