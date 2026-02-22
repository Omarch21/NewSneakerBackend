using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace SneakerWebAPI.Models.Sneaker
{
    public class Sneaker :  BaseItem
    {
        public string Brand { get; set; } = string.Empty;
        public string Silhouette { get; set; } = string.Empty;
        public float Size { get; set; }
        public string? Colorway { get; set; }
        public string? SKU { get; set; }
        public string? ReleaseDate { get; set; }
        public string? ProductDesc { get; set; }
        public string? Creator { get; set; }
        public bool? Holding { get; set; }
        public override string ItemType => "Sneaker";
        public int SiteId { get; set; }

    }
}
