using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using System.Text;

namespace SneakerWebAPI
{
    public class SneakerPricePoster : IJob
    {
        private readonly string apistring = "https://localhost:7017/api/SnkrPriceHistories";
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var web = await httpClient.GetAsync(apistring);
                    var stringContent = new StringContent(JsonConvert.SerializeObject(web), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apistring, stringContent);
                    response.EnsureSuccessStatusCode();
                    
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
