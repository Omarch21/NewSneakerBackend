using Microsoft.OpenApi.Any;
using Quartz;
using System.Threading.Tasks;
using System.Net.Http;

namespace SneakerWebAPI
{
    public class ResellUpdater : IJob
    {
        private readonly string apistring = "http://localhost:5001/api/SneakerResellUpdater";

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    //var shoe = new StringContent("", Encoding.UTF8, "application/json");
                    var web = await httpClient.PutAsync(apistring,null);
                    Console.WriteLine(web);
                    

                }
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
