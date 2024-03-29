using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace SneakerWebAPI.Services.SneakerService
{
    public class SneakerService : ISneakerService
    {
        private readonly HttpClient _client;
        public SneakerService(HttpClient client)
        {
            _client = client;
        }
        public float GetPrice(string size1, string url)
        {

            
                var size = size1;
                string source = url;
                var response = _client.GetAsync(source).Result;
                string shoe = response.Content.ReadAsStringAsync().Result;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(shoe);
                float price = 0;

                string pricefinder = "//script[contains(.,'\"size\":" + size + "')][contains(.,'\"price\":')]";
                HtmlNode scriptNode = document.DocumentNode.SelectSingleNode(pricefinder);

                if (scriptNode != null)
                {
                    // Get the JSON part from the script content
                    string jsonString = scriptNode.InnerText;

                    // Parse the JSON to a JObject
                    JObject jsonObj = JObject.Parse(jsonString);

                    // Get the offers array from the JSON object
                    JArray offersArray = jsonObj["offers"]["offers"] as JArray;

                    if (offersArray != null)
                    {
                        // Find the offer for size 8.5
                        JObject offerForSize8_5 = offersArray.FirstOrDefault(o => o["size"].Value<string>() == "8.5") as JObject;

                        if (offerForSize8_5 != null)
                        {
                            // Get the price for size 8.5 from the offer object
                            string size8_5Price = offerForSize8_5["price"].Value<string>();
                            price = float.Parse(size8_5Price);
                            
                            return price;
                        }
                        else
                        {
                            return 0 ;
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                 return -2;
                }


            



        
        }
    }
}
