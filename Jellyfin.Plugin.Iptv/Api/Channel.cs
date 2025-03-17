using System.Collections.ObjectModel;


/*
  "id": "002RadioTV.do",
        "name": "002 Radio TV",
        "alt_names": [],
        "network": null,
        "owners": [],
        "country": "DO",
        "subdivision": null,
        "city": "Santo Domingo",
        "broadcast_area": ["c/DO"],
        "languages": ["spa"],
        "categories": ["general"],
        "is_nsfw": false,
        "launched": null,
        "closed": null,
        "replaced_by": null,
        "website": "https://www.002radio.com/",
        "logo": "https://i.imgur.com/7oNe8xj.png"
 */

namespace Jellyfin.Plugin.Iptv.Api
{
    // TODO support regions and subdivisions
    public class Channel : IIptvEntity
    {
        private Collection<string> countries;

        public Channel()
        {
            Languages = [];
            Categories = [];
            BroadcastArea = [];
            HasStream = false;
            HasGuide = false;
        }

        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string Website { get; set; }

        public required string Logo { get; set; }

        public required string Country { get; set; }

        public bool HasStream { get; set; }

        public bool HasGuide { get; set; }

        public string? Network { get; set; }

        public string? Subdivision { get; set; }

        public Collection<string> Languages { get; set; }

        public Collection<string> Categories { get; set; }

        public Collection<string> BroadcastArea { get; set; }

        public Collection<string> Countries
        {
            get
            {
                if (countries == null)
                {
                    countries = [];
                    foreach (string area in this.BroadcastArea)
                    {
                        // Find list of countries in broadcast areas
                        if (area.StartsWith('c'))
                        {
                            countries.Add(area.Substring(2).ToLowerInvariant());
                        }
                    }
                }
                return countries;
            }
        }

        public virtual string Key
        {
            get
            {
                return Id;
            }
        }

        public override string ToString()
        {
            return "{id=" + Id + ",name=" + Name + ",cats=[" + string.Join(",", Categories) + "],langs=[" + string.Join(",", Languages) + "]}";
        }

    }
}
