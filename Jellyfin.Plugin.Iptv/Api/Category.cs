namespace Jellyfin.Plugin.Iptv.Api
{
    public class Category : IIptvEntity
    {
        public required string Id { get; set; }

        public virtual string Key
        {
            get
            {
                return Id;
            }
        }
    }
}
