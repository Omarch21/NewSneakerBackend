using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerWebAPI.Models.FunkoPop
{
    public class FunkoPopPrice : ItemPrice
    {
        [ForeignKey("FunkoPopId")]
        public FunkoPop FunkoPop { set; get; }
        public int FunkoPopId { set; get; }
    }
}
