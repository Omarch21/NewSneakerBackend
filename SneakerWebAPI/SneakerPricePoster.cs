using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using SneakerWebAPI.Services.SneakerService;
using System.Text;

namespace SneakerWebAPI
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
        /*private static void PerformInitialPlaywrightInstall()
        {
            try
            {
                var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "--with-deps", "chromium" });
                if (exitCode != 0)
                {
                    throw new Exception($"Playwright installation failed with exit code {exitCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during Playwright installation: {ex.Message}");
            }
        }*/
    }
}
