using SneakerWebAPI.Models.Card;

namespace SneakerWebAPI.Services.CardService
{
    public interface ICardService
    {

        Task<List<FetchedCardData>> SearchCardsInSite(string search);
        Task<List<CardPrice>> PostCardPrices();
        Task<float> GetPrice(string url);
    }
}
