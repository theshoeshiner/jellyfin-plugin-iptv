namespace Jellyfin.Plugin.Iptv.Api
{
    public class Stream : IIptvEntity
    {

        public required string Channel { get; set; }

        public required string Url { get; set; }

        public string? Timeshift { get; set; }

        public string? Quality { get; set; }

        public string? Tags { get; set; }

        public string? HttpReferrer { get; set; }

        public string? UserAgent { get; set; }

        public virtual string Key
        {
            get
            {
                return Channel;
            }
        }
    }
}
