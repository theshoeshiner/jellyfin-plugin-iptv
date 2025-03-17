using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.Iptv.Api
{
    public class PlaylistEntry
    {

        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string Url { get; set; }

        public string? Logo { get; set; }

        public string? Group { get; set; }

    }
}
