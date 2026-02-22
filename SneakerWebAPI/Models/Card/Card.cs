using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace SneakerWebAPI.Models.Card
{
    public class Card : BaseItem
    {
        public string CardGame { get; set; } = string.Empty;
        public string? CardType { get; set; } = string.Empty;
        public string? Set { get; set; } = string.Empty;
        public string? Rarity { get; set; }
        public override string ItemType => "Card";
    }
}
