namespace SneakerWebAPI.DTOs
{
    public class SneakerReleaseResponse
    {
        public string Name { get; set; } = string.Empty;
        public int Retail { get; set; }
        public string ReleaseDate { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string MoreInfoSource { get; set; } = string.Empty;
    }
}
