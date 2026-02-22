using Quartz;
using SneakerWebAPI.Services.GameService;

namespace SneakerWebAPI.ScheduledTasks
{
    public class GamePricePoster : IJob
    {
        private readonly IGameService _GameService;
        public GamePricePoster(IGameService GameService)
        {
            _GameService = GameService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                //PerformInitialPlaywrightInstall();
                await _GameService.PostGamePrices();
                Console.WriteLine("Prices have been posted");
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
