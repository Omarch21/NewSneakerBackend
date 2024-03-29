using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace SneakerWebAPI
{
    public class Sneaker
    {
        public int Id {  get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Silhouette { get; set; } = string.Empty;
        public int Retail { get; set; }

        public string? PhotoURL { get; set; } = "";

        public float? ResellPrice { get; set; }
        public float Size { get; set; } 
        public string? Colorway { get; set; }
        public string? ResellURL { get; set; }
        public string? SKU { get; set; }
        public string? ReleaseDate { get; set; }
        public string? ProductDesc { get; set; }
        public string? Creator { get; set; }

        [ForeignKey("UserID")]
        public User? user { get; set; }
        public int UserID { get; set; }
        public Boolean? Holding { get; set; }

    }
}
