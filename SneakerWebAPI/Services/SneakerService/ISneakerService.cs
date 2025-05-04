namespace SneakerWebAPI.Services.SneakerService
{
    public interface ISneakerService
    {
        Task<float> GetPrice(string size,string url);
        Task<Sneaker> GetSneakerInfo(string url);
        Task<List<SneakerPriceHistory>> PostSneakerPrices();
    }
}
