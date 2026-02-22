using SneakerWebAPI.Models.Card;
using SneakerWebAPI.Models.FunkoPop;

namespace SneakerWebAPI.Services.FunkoPopService
{
    public interface IFunkoPopService
    {

        Task<List<FetchedFunkoPopData>> SearchFunkoPopsInSite(string search);
        Task<List<FunkoPopPrice>> PostFunkoPopPrices();
        Task<float> GetPrice(string url);
    }
}
