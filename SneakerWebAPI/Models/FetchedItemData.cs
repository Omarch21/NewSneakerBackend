namespace SneakerWebAPI.Models
{
    public abstract class FetchedItemData
    {
        public string Url { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;
    }
}
