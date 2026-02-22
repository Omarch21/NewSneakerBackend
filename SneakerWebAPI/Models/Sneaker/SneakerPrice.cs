using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI.Models.Sneaker
{
    public class SneakerPrice : ItemPrice
    {
        [ForeignKey("SneakerId")]
        public Sneaker Sneaker { set; get; }
        public int SneakerId { set; get; }
    }
}
