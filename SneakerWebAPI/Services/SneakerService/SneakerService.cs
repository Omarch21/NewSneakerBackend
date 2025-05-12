using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using SneakerWebAPI.Data;
using System.Net.Http;

namespace SneakerWebAPI.Services.SneakerService
{
    public class SneakerService : ISneakerService
    {
        private readonly HttpClient _client;
        private readonly IPlaywright _playwright;
        private readonly DataContext _context;
        public SneakerService(HttpClient client, DataContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<float> GetPrice(string size, string url)
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

                var scriptNode = document.DocumentNode.SelectSingleNode(
                    $"//script[contains(., '\"size\":{size}') and contains(., '\"price\":')]"
                );
                if (scriptNode == null)
                    return -2; // Script not found

                // Extract JSON content from the script (you may need to extract actual JSON string)
                string jsonText = scriptNode.InnerText.Trim();

                // Try to parse the script content into JSON
                JObject jsonObj = JObject.Parse(jsonText);

                var offersArray = jsonObj["offers"]?["offers"] as JArray;
                if (offersArray == null)
                    return -1; // Offers not found

                var offer = offersArray
                    .FirstOrDefault(o => o["size"]?.ToString() == size) as JObject;

                if (offer == null)
                    return 0; // No matching size

                var priceStr = offer["price"]?.ToString();
                return float.TryParse(priceStr, out float price) ? price : 0;
                /*var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(html);

                // Look for the script tag containing both "size" and "price"
                var scriptNode = document.DocumentNode.SelectSingleNode(
                    $"//script[contains(., '\"size\":{size}') and contains(., '\"price\":')]"
                );

                if (scriptNode == null)
                    return -2; // Script not found

                // Extract JSON content from the script (you may need to extract actual JSON string)
                string jsonText = scriptNode.InnerText.Trim();

                // Try to parse the script content into JSON
                JObject jsonObj = JObject.Parse(jsonText);

                var offersArray = jsonObj["offers"]?["offers"] as JArray;
                if (offersArray == null)
                    return -1; // Offers not found

                var offer = offersArray
                    .FirstOrDefault(o => o["size"]?.ToString() == size) as JObject;

                if (offer == null)
                    return 0; // No matching size

                var priceStr = offer["price"]?.ToString();
                return float.TryParse(priceStr, out float price) ? price : 0;*/
            }
            catch (Exception ex)
            {
                // You could log the exception here
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
                return -999; // General failure
            }
        }

        public async Task<Sneaker> GetSneakerInfo(string url)
        {
            if (string.IsNullOrEmpty(url))
                return new Sneaker();

            Sneaker sneaker = new Sneaker();
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
            {
                var page = await browser.NewPageAsync();
                await page.GoToAsync(url);
                await page.WaitForSelectorAsync("div[data-qa='ProductDescriptionText'] p");

                var elementWithDescription = await page.QuerySelectorAsync("div[data-qa='ProductDescriptionText'] p");
                sneaker.ProductDesc = await elementWithDescription.EvaluateFunctionAsync<string>("el => el.innerText");

                var elementWithColorway = await page.QuerySelectorAsync("span[data-qa='ProductColorway']");
                var colorway = await elementWithColorway.EvaluateFunctionAsync<string>("el => el.innerText");
                int index = colorway.IndexOf(": ") + 2;
                sneaker.Colorway = colorway.Substring(index);

                var elementWithReleaseDate = await page.QuerySelectorAsync("span[data-qa='ProductReleaseDate']");
                var releaseDate = await elementWithReleaseDate.EvaluateFunctionAsync<string>("el => el.innerText");
                index = releaseDate.IndexOf(": ") + 2;
                sneaker.ReleaseDate = releaseDate.Substring(index);

                var elementWithSKU = await page.QuerySelectorAsync("span[data-qa='ProductSku']");
                var sku = await elementWithSKU.EvaluateFunctionAsync<string>("el => el.innerText");
                index = sku.IndexOf(": ") + 2;
                sneaker.SKU = sku.Substring(index);
            }
            return sneaker;
        }

        public async Task<List<SneakerPriceHistory>> PostSneakerPrices()
        {
            var shoes = await _context.Sneakers.ToListAsync();
            List<SneakerPriceHistory> pricelist = new List<SneakerPriceHistory>();
            if (shoes == null)
                return new List<SneakerPriceHistory> { };

            foreach (var shoe1 in shoes)
            {
                string size = shoe1.Size.ToString();
                string source = shoe1.ResellURL;
                float price = await GetPrice(size, source);
                shoe1.ResellPrice = price;
                SneakerPriceHistory history = new SneakerPriceHistory()
                {
                    SneakerId = shoe1.Id,
                    Date = DateTime.Now,
                    Price = price
                };

                if (history != null)
                {
                    history.Price = price;
                    pricelist.Add(history);
                    _context.SneakerPrices.Add(history);
                    await _context.SaveChangesAsync();
                }
            }
            return pricelist;
        }
    }
}