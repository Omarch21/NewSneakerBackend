using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using SneakerWebAPI.Data;
using SneakerWebAPI.Models.Card;
using SneakerWebAPI.Models.FunkoPop;
using System.Net;
using System.Text.Json;

namespace SneakerWebAPI.Services.FunkoPopService
{
    public class FunkoPopService : IFunkoPopService
    {
        private readonly HttpClient _client;
        private readonly DataContext _context;
        public FunkoPopService(HttpClient client, DataContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<List<FetchedFunkoPopData>> SearchFunkoPopsInSite(string search)
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
                                loosePrice: el.querySelector('td.used_price span')?.textContent.trim() || 'No price',
                                cibPrice: el.querySelector('td.cib_price span')?.textContent.trim() || 'No price',
                                newPrice: el.querySelector('td.new_price span')?.textContent.trim() || 'No price',
                                })); return JSON.stringify(results)}");

            var items = JsonSerializer.Deserialize<List<FetchedFunkoPopData>>(jsondata, new JsonSerializerOptions
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
                var priceStr = priceContainerNode.SelectSingleNode(".//span[contains(@class, 'price')]").InnerText.Trim().Replace("$", "") ?? "";
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
        public async Task<List<FunkoPopPrice>> PostFunkoPopPrices()
        {
            var pops = await _context.FunkoPops.ToListAsync();
            List<FunkoPopPrice> priceList = new List<FunkoPopPrice>();
            if (pops == null)
                return new List<FunkoPopPrice> { };

            foreach (var pop in pops)
            {
                var popPrice = await GetPrice(pop.ResellURL);
                pop.ResellPrice = popPrice;

                FunkoPopPrice price = new FunkoPopPrice()
                {
                    FunkoPopId = pop.Id,
                    Date = DateTime.Now,
                    Price = popPrice
                };

                if (price != null)
                {
                    priceList.Add(price);
                    _context.FunkoPopPrices.Add(price);
                    await _context.SaveChangesAsync();
                }
            }
            return priceList;
        }
    }

}