using SneakerWebAPI.Models;
using SneakerWebAPI.Models.Sneaker;

namespace SneakerWebAPI.Services.SneakerService
{
    public interface ISneakerService
    {
        Task<float> GetPrice(string size,string url);
        Task<Sneaker> GetSneakerInfo(string url);
        Task<List<SneakerPrice>> PostSneakerPrices();
        Task<List<FetchedSneakerData>> SearchSneakersInSite(string search);
        Task<List<FetchedSneakerData>> SearchSneakersInSiteGoat(string search);
        Task<float> GetUsedPrice(string url, double size, string condition);
        Task<Sneaker> GetSneakerInfoGoat(string url);
    }
}
