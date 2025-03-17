namespace Jellyfin.Plugin.Iptv.Api
{
    public class Guide : IIptvEntity
    {
        public required string? Channel { get; set; }

        public required string Site { get; set; }

        public required string SiteId { get; set; }

        public required string SiteName { get; set; }

        public required string Lang { get; set; }

        public virtual string Key
        {
            get
            {
                return Channel;
            }
        }
    }
}
