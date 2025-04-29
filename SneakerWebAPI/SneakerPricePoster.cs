using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using System.Text;

namespace SneakerWebAPI
{
    public class SneakerPricePoster : IJob
    {
        private readonly string apistring = "http://localhost:5000/api/SneakerPriceHistories";
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apistring,new StringContent(""));
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("Prices have been posted");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
