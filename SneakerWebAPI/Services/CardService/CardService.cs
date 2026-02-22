using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using SneakerWebAPI.Data;
using SneakerWebAPI.Models.Card;
using System.Net;
using System.Text.Json;

namespace SneakerWebAPI.Services.CardService
{
    public class CardService : ICardService
    {
        private readonly HttpClient _client;
        private readonly DataContext _context;
        public CardService(HttpClient client, DataContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<List<FetchedCardData>> SearchCardsInSite(string search)
        {
            string searchEncoded = WebUtility.UrlEncode(search);
            string searchUrl = $"https://www.pricecharting.com/search-products?type=prices&q={searchEncoded}&go=Go";

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
                Locale = "en-US"
            });
            var page = await context.NewPageAsync();

            await page.GotoAsync(searchUrl);
            await page.WaitForSelectorAsync("tr[id^='product-']");
            var jsondata = await page.EvaluateAsync<string>(@"
                                () => {
                                const elements = Array.from(document.querySelectorAll('tr[id^=""product-""]'));
                                const results =  elements.slice(0,3).map(el=>({
                                url: el.querySelector('td.title a')?.href || 'No link',
                                imageLink: el.querySelector('td.image img')?.src || 'No image',
                                set: el.querySelector('td.console a')?.textContent.trim() || 'No set',
                                name: el.querySelector('td.title a')?.textContent.trim() || 'No title',
                                cost: el.querySelector('td.used_price span')?.textContent.trim() || 'No price'
                                })); return JSON.stringify(results)}");

            var items = JsonSerializer.Deserialize<List<FetchedCardData>>(jsondata, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return items;
        }
        public async Task<float> GetPrice(string url)
        {
            try
            {

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                    //ExecutablePath = "/root/.cache/ms-playwright/chromium-1169/chrome-linux/chrome"
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
                    ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
                    Locale = "en-US"
                });
                var page = await context.NewPageAsync();

                await page.GotoAsync(url);

                var html = await page.ContentAsync();
                var document = new HtmlDocument();
                document.LoadHtml(html);

                var priceContainerNode = document.DocumentNode.SelectSingleNode("//td[@id='used_price']");
                
                var priceStr = priceContainerNode.SelectSingleNode(".//span[contains(@class, 'price')]").InnerText.Trim().Replace("$","") ?? "";
                return float.TryParse(priceStr, out float price) ? price : 0;
            }
            catch (Exception ex)
            {
                // You could log the exception here
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
                return -999; // General failure
            }
        }
        public async Task<List<CardPrice>> PostCardPrices()
        {
            var cards = await _context.Cards.ToListAsync();
            List<CardPrice> pricelist = new List<CardPrice>();
            if (cards == null)
                return new List<CardPrice> { };

            foreach (var card in cards)
            {
                var cardPrice = await GetPrice(card.ResellURL);
                card.ResellPrice = cardPrice;

                CardPrice price = new CardPrice()
                {
                    CardId = card.Id,
                    Date = DateTime.Now,
                    Price = cardPrice
                };

                if (price != null)
                {
                    pricelist.Add(price);
                    _context.CardPrices.Add(price);
                    await _context.SaveChangesAsync();
                }
            }
            return pricelist;
        }
    }
    
}