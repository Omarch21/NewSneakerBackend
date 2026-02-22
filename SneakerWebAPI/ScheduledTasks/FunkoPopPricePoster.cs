using Quartz;
using SneakerWebAPI.Services.FunkoPopService;
using SneakerWebAPI.Services.SneakerService;

namespace SneakerWebAPI.ScheduledTasks
{
    public class FunkoPopPricePoster : IJob
    {
        private readonly IFunkoPopService _funkoPopService;
        public FunkoPopPricePoster(IFunkoPopService funkoPopService)
        {
            _funkoPopService = funkoPopService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                //PerformInitialPlaywrightInstall();
                await _funkoPopService.PostFunkoPopPrices();
                Console.WriteLine("Prices have been posted for pops.");
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
