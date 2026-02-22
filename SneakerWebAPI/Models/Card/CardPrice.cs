using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI.Models.Card
{
    public class CardPrice : ItemPrice
    {
            [ForeignKey("CardId")]
            public Card Card { set; get; }
            public int CardId { set; get; }
    }
}
