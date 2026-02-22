using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using SneakerWebAPI.Services.SneakerService;
using System.Text;

namespace SneakerWebAPI.ScheduledTasks
{
    public class SneakerPricePoster : IJob
    {
        private readonly ISneakerService _sneakerService;
        public SneakerPricePoster(ISneakerService sneakerService)
        {
            _sneakerService = sneakerService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                //PerformInitialPlaywrightInstall();
                await _sneakerService.PostSneakerPrices();
                Console.WriteLine("Prices have been posted");
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
