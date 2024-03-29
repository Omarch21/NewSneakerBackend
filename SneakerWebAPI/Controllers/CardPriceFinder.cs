using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI;
using SneakerWebAPI.Data;
using HtmlAgilityPack;
using Microsoft.Identity.Client;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Humanizer;
using System.Net.Http;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardPriceFinderController : ControllerBase
    {

        private readonly HttpClient _httpclient;
        private readonly DataContext _context;
        public CardPriceFinderController(DataContext context)
        {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _context = context;
        }

        /*
        public async Task<int> GetCardPrices(string cardResellUrl)
        {
           

               // var game = card.CardGame;
                string source = cardResellUrl;
                   // card.ResellURL;
                var response = await _httpclient.GetAsync(source);
                string card_site = await response.Content.ReadAsStringAsync();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(card_site);
                
                string pricefinder = "//span[@id='sale-price']";
                HtmlNode scriptNode = document.DocumentNode.SelectSingleNode(pricefinder);
                
                if (scriptNode != null)
                {
                    
                    string price = scriptNode.InnerText;
                    Console.WriteLine(price);
                // Parse the JSON to a JObject
                return price;
                }
                else
                {
                    

                    return NotFound("The script tag containing the required conditions not found on the page.");
                  
                }

            }
        
        
        */



        [HttpPut]
        public async Task<ActionResult<List<Sneaker>>> UpdateSneaker(int id)
        {


            var shoe1 = await _context.Sneakers.FindAsync(id);
            if ((shoe1 == null))
            {
                return BadRequest("Sneaker was not Found");
            }
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
                    JObject offerForSize8_5 = offersArray.FirstOrDefault(o => o["size"].Value<string>() == "8.5") as JObject;

                    if (offerForSize8_5 != null)
                    {
                        // Get the price for size 8.5 from the offer object
                        string size8_5Price = offerForSize8_5["price"].Value<string>();
                        shoe1.ResellPrice = float.Parse(size8_5Price);
                        await _context.SaveChangesAsync();
                        return Ok(size8_5Price);
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



    }
}

