using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Jellyfin.Plugin.Iptv.Configuration
{
    public class ChannelFilter
    {

        public ChannelFilter()
        {
            Languages = [];
            Categories = [];
            Countries = [];
            Regions = [];
        }


        public Collection<string> Languages { get; set; }

        public Collection<string> Categories { get; set; }

        public Collection<string> Countries { get; set; }

        public Collection<string> Regions { get; set; }

        public override string ToString()
        {

            return "{countries=[" + string.Join(",", Countries) + "],regions=[" + string.Join(",", Regions) + "],cats=[" + string.Join(",", Categories) + "],langs=[" + string.Join(",", Languages) + "]}";
        }

        //public Collection<string>? Regions { get; set; }

    }
}
