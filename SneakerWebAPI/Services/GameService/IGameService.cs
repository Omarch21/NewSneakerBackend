using SneakerWebAPI.Models.Game;

namespace SneakerWebAPI.Services.GameService
{
    public interface IGameService
    {

        Task<List<FetchedGameData>> SearchGamesInSite(string search);
        Task<List<GamePrice>> PostGamePrices();
        Task<float> GetPrice(string url, string condition);
        Task AssignMoreData(string url, Game game);
    }
}
