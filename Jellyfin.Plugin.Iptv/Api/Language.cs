namespace Jellyfin.Plugin.Iptv.Api
{
    public class Language : IIptvEntity
    {
        public required string Code { get; set; }

        public required string Name { get; set; }

        public virtual string Key
        {
            get
            {
                return Code;
            }
        }
    }
}
