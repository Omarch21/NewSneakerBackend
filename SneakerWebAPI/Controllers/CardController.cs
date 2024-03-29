using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using SneakerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Services.CardService;
using static System.Net.WebRequestMethods;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpclient;
        private readonly ICardService _cardService;
  
        public CardController(DataContext context,HttpClient client,ICardService cardService)
        {
            _context = context;
            _httpclient = client;
            _cardService = cardService;
        }
        [HttpPost]
        public async Task<ActionResult> AddCard(Card card)
        {
            
         
           if (card != null)
            {
                float cardprice = _cardService.GetPrice(card.ResellURL);
                card.price = cardprice;
               _context.Cards.Add(card);
                Console.WriteLine(cardprice);
               await _context.SaveChangesAsync();
                return Ok(cardprice);
            }
            else
            {
                return BadRequest();
            }


        }
        
        private string GetCardPrices(string cardResellUrl)
        {


            // var game = card.CardGame;
            string source = cardResellUrl;
            // card.ResellURL;
            var response = _httpclient.GetAsync(source).Result;
            string card_site = response.Content.ReadAsStringAsync().Result;
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


                return "0";

            }

        }

    }
}
