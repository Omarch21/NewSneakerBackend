namespace SneakerWebAPI.DTOs
{
    public class ItemSoldRequest
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; } = string.Empty;
    }
}
