using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI
{
    public class SneakerPriceHistory
    {
        public int Id { set; get; }
        [ForeignKey("SneakerId")]
        public Sneaker sneaker { set; get; }
        public int SneakerId { set; get; }
        public DateTime Date { set; get; }
        public float Price { set; get; }
    }
}
