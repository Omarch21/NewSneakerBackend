namespace SneakerWebAPI.Models.Sneaker
{
    public class FetchedSneakerData : FetchedItemData
    {
        public string Brand { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public int JordanNumber { get; set; }
        public string? SiteId { get; set; } = string.Empty;
        public string? silhouette { get; set; } = string.Empty;
    }
}
