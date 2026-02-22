using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using SneakerWebAPI.Data;
using SneakerWebAPI.Models.Game;
using System.Net;
using System.Text.Json;
using System.Web;

namespace SneakerWebAPI.Services.GameService
{
    public class GameService : IGameService
    {
        private readonly HttpClient _client;
        private readonly DataContext _context;
        public GameService(HttpClient client, DataContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<List<FetchedGameData>> SearchGamesInSite(string search)
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
            var jsondata = await page.EvaluateAsync<string>(@"
                                () => {
                                const elements = Array.from(document.querySelectorAll('tr[id^=""product-""]'));
                                const results =  elements.slice(0,3).map(el=>({
                                url: el.querySelector('td.title a')?.href || 'No link',
                                imageLink: el.querySelector('td.image img')?.src || 'No image',
                                name: el.querySelector('td.title a')?.textContent.trim() || 'No title',
                                console: el.querySelector('td.console a')?.textContent.trim() || 'No Console',
                                loosePrice: el.querySelector('td.used_price span')?.textContent.trim() || 'No price',
                                cibPrice: el.querySelector('td.cib_price span')?.textContent.trim() || 'No price',
                                newPrice: el.querySelector('td.new_price span')?.textContent.trim() || 'No price',
                                })); return JSON.stringify(results)}");

            var items = JsonSerializer.Deserialize<List<FetchedGameData>>(jsondata, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return items;
        }
        public async Task<float> GetPrice(string url, string condition)
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

                var conditionMap = new Dictionary<string, string>
                {
                    { "CIB", "complete_price" },
                    { "Loose", "used_price" },
                    { "New", "new_price" }
                };
                conditionMap.TryGetValue(condition, out string conditionNode);

                var priceContainerNode = document.DocumentNode.SelectSingleNode($"//td[@id='{conditionNode}']");
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
        public async Task<List<GamePrice>> PostGamePrices()
        {
            var games = await _context.Games.ToListAsync();
            List<GamePrice> pricelist = new List<GamePrice>();
            if (games == null)
                return new List<GamePrice> { };

            foreach (var game in games)
            {
                var gamePrice = await GetPrice(game.ResellURL, game.Condition);
                game.ResellPrice = gamePrice;

                GamePrice price = new GamePrice()
                {
                    GameId = game.Id,
                    Date = DateTime.Now,
                    Price = gamePrice
                };

                if (price != null)
                {
                    pricelist.Add(price);
                    _context.GamePrices.Add(price);
                    await _context.SaveChangesAsync();
                }
            }
            return pricelist;
        }


        public async Task AssignMoreData(string url, Game game)
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

            var detailsNode = document.DocumentNode.SelectSingleNode($"//div[@id='full_details']");
            var genreStr = detailsNode.SelectSingleNode(".//td[@itemprop='genre']").InnerText.Trim();
            var releaseDateStr = detailsNode.SelectSingleNode(".//td[@itemprop='datePublished']").InnerText.Trim();
            var ratingStr = detailsNode.SelectSingleNode(".//td[@itemprop='contentRating']").InnerText.Trim();
            var publisherStr = detailsNode.SelectSingleNode(".//td[@itemprop='publisher']").InnerText.Trim();

            game.Genre = HttpUtility.HtmlDecode(genreStr);
            game.ReleaseDate = DateTime.TryParse(releaseDateStr, out DateTime releaseDate) ? releaseDate : null;
            game.Rating = ratingStr;
            game.Publisher = publisherStr;
        }
    }

}