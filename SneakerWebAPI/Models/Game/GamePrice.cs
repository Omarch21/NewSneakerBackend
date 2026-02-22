using SneakerWebAPI.Models.Game;
using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI.Models.Game
{
    public class GamePrice : ItemPrice
    {
        [ForeignKey("GameId")]
        public Game Game { set; get; }
        public int GameId { set; get; }
    }
}
