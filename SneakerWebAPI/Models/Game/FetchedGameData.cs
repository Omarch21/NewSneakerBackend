namespace SneakerWebAPI.Models.Game
{
    public class FetchedGameData :  FetchedItemData
    {
        public string Console { get; set; } = string.Empty;
        public string LoosePrice { get; set; } = string.Empty;
        public string CIBPrice { get; set; } = string.Empty;
        public string NewPrice { get; set; } = string.Empty;    
    }
}
