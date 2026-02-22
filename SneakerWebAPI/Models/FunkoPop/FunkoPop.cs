using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace SneakerWebAPI.Models.FunkoPop
{
    public class FunkoPop :  BaseItem
    {
        public string? ReleaseDate { get; set; }
        public string? Size { get; set; }
        public override string ItemType => "Funko Pop";

    }
}
