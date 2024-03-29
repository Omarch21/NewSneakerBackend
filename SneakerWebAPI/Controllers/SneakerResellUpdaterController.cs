using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SneakerWebAPI;
using SneakerWebAPI.Data;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SneakerResellUpdaterController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpclient;

        public SneakerResellUpdaterController(DataContext context)
        {
            _context = context;
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }
        [HttpPut]

        public async Task<ActionResult<List<Sneaker>>> UpdateAllSneakers()
        {


            var shoes = await _context.Sneakers.ToListAsync();
            if ((shoes == null))
            {
                return BadRequest("Sneaker was not Found");
            }
            foreach (var shoe1 in shoes)
            {
                var size = shoe1.Size;
                string source = shoe1.ResellURL;
                var response = await _httpclient.GetAsync(source);
                string shoe = await response.Content.ReadAsStringAsync();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(shoe);

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
                        JObject offerForSize8_5 = offersArray.FirstOrDefault(o => o["size"].Value<string>() == size.ToString()) as JObject;

                        if (offerForSize8_5 != null)
                        {
                            
                            // Get the price for size 8.5 from the offer object
                            string size8_5Price = offerForSize8_5["price"].Value<string>();
                            Console.WriteLine(size8_5Price);
                            shoe1.ResellPrice = float.Parse(size8_5Price);
                        }
                        else
                        {
                            return NotFound("Price for size 8.5 not found in the script content.");
                        }
                    }
                    else
                    {
                        return NotFound("Offers data not found in the script content.");
                    }
                }
                else
                {
                    return NotFound("The script tag containing the required conditions not found on the page.");
                }
                
            }
            try
            {
                await _context.SaveChangesAsync();
            }catch(Exception Error)
            {
                Console.WriteLine(Error.Message);
            }
            return Ok();
        }
    }
}
