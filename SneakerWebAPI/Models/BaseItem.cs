using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI.Models
{
    public abstract class BaseItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoURL { get; set; }
        public string? ResellURL { get; set; }
        public int Cost { get; set; }
        public float? ResellPrice { get; set; }
        public string? Condition { get; set; } = string.Empty;
        public string? PurchasedFrom { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public int UserId { get; set; }
        public bool Sold { get; set; }
        [NotMapped]
        public abstract string ItemType { get; }
    }
}
