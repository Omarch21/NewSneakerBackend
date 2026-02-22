namespace SneakerWebAPI.Models.Sneaker
{
    public class UsedSneaker
    {
        public string BoxCondition { get; set; } = string.Empty;
        public float PriceCents { get; set; }
        public string OuterPictureUrl { get; set; } = string.Empty;
    }
}
