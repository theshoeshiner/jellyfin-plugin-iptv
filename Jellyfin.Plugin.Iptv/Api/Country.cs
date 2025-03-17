using System.Collections.ObjectModel;

namespace Jellyfin.Plugin.Iptv.Api
{

    public class Country : IIptvEntity
    {

        public required string Code { get; set; }

        public required string Name { get; set; }

        public required string Flag { get; set; }

        public required Collection<string> Languages { get; set; }

        public virtual string Key
        {
            get
            {
                return Code;
            }
        }
    }
}
