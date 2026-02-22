using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI.Models.Game
{
    public class Game : BaseItem
    {
        public string Console { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? Rating { get; set; } = string.Empty;
        public string? Publisher { get; set; } = string.Empty;
        public override string ItemType => "Game";
    }
}
