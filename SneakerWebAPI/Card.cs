using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace SneakerWebAPI
{
    public class Card
    {
        public int Id { get; set; }
        public string CardGame { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public float price { get; set; }
        public string? PhotoURL { get; set; }
        public string? ResellURL { get; set; }
        public string? Rarity { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
    }
}
