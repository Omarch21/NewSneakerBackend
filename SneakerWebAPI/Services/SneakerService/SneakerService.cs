using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using SneakerWebAPI.Data;
using SneakerWebAPI.Models;
using SneakerWebAPI.Models.Sneaker;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                             "AppleWebKit/537.36 (KHTML, like Gecko) " +
                             "Chrome/120.0.0.0 Safari/537.36");
                await page.GoToAsync(url);
                var a = await page.GetContentAsync();
                await page.WaitForSelectorAsync("div.ProductDescription__ProductDescriptionLayout-fc-web__sc-wcuf3k-0");

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

        public async Task<List<SneakerPrice>> PostSneakerPrices()
        {
            var shoes = await _context.Sneakers.ToListAsync();
            List<SneakerPrice> pricelist = new List<SneakerPrice>();
            if (shoes == null)
                return new List<SneakerPrice> { };

            foreach (var shoe1 in shoes)
            {
                string size = shoe1.Size.ToString();
                string source = shoe1.ResellURL;
                float price = 0;
                if(new[] { "Used", "Badly Damaged", "No Original Box" }.Contains(shoe1.Condition))
                    price = await GetUsedPrice(shoe1.SiteId.ToString(), shoe1.Size, shoe1.Condition);
                else
                    price = await GetPrice(size, source);

                shoe1.ResellPrice = price;
                SneakerPrice history = new SneakerPrice()
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

        public async Task<List<FetchedSneakerData>> SearchSneakersInSite(string search)
        {
            string searchQuery = search;
            string searchUrl = $"https://www.flightclub.com/catalogsearch/result?query={Uri.EscapeDataString(searchQuery)}";

            //await new BrowserFetcher().DownloadAsync();
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
            await page.WaitForSelectorAsync(".LinkCTA__StyledLink-fc-web__sc-1wc5x2x-0");
            var jsondata = await page.EvaluateAsync<string>(@"
                                () => {
                                const elements = Array.from(document.querySelectorAll('.LinkCTA__StyledLink-fc-web__sc-1wc5x2x-0'));
                                const results = elements.slice(0,3).map(el=>({
                                url: el.href || 'No link',
                                imageLink: el.querySelector('img')?.src || 'No image',
                                cost: el.querySelector('[data-qa=""ProductItemPrice""]')?.textContent.trim() || 'No cost',
                                name: el.querySelector('[data-qa=""ProductItemTitle""]')?.textContent.trim() || 'No name',
                                brand: el.querySelector('[data-qa=""ProductItemBrandName""]')?.textContent.trim() || 'No brand'
                                }));return JSON.stringify(results)}");
            var items = JsonSerializer.Deserialize<List<FetchedSneakerData>>(jsondata, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            string pattern = @"'([^']*)'";
            foreach (var item in items)
            {
                string[] jordanNumber = item.Name.Split(' ');
                int jordanNumberIndex = Array.IndexOf(jordanNumber, "Jordan");
                item.JordanNumber = (jordanNumberIndex >= 0 && int.TryParse(jordanNumber[++jordanNumberIndex], out var jNum)) ? jNum : 0;
                Match match = Regex.Match(item.Name, pattern);
                item.Nickname = match.Groups[1].Value;
            }
            return items;
            
        }

        public async Task<List<FetchedSneakerData>> SearchSneakersInSiteGoat(string search)
        {
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
            List<FetchedSneakerData> fetchedSneakers = new List<FetchedSneakerData>();
            var tcs = new TaskCompletionSource<bool>();

            await page.GotoAsync($"https://www.goat.com/search?query={Uri.EscapeDataString(search)}");
            var response = await page.WaitForResponseAsync(r => r.Url.Contains("get-product-search-results") && r.Status == 200);

            try
                    {
                        var body = await response.TextAsync();
                        var json = JsonDocument.Parse(body);

                        if(json.RootElement.TryGetProperty("data", out var data) &&
                            data.TryGetProperty("productsList", out var products))
                        {
                            foreach(var product in (products.EnumerateArray().Take(3)))
                            {
                                var goatId = product.GetProperty("id").GetString();
                                var title = product.GetProperty("title").GetString();
                                var silhouette = product.GetProperty("silhouette").GetString();
                                var pictureUrl = product.GetProperty("pictureUrl").GetString().Replace(".com/",".com/transform/v1/")+"?action=crop&width=750";
                                var url = $"https://goat.com/sneakers/{product.GetProperty("slug").GetString()}";
                                var brand = product.GetProperty("brandName").GetString();

                                DateTime? releaseDate = null;
                                if (product.TryGetProperty("releaseDate", out var rd) &&
                                    rd.TryGetProperty("seconds", out var seconds))
                                {
                                    releaseDate = DateTimeOffset.FromUnixTimeSeconds(seconds.GetInt64()).Date;
                                }

                                int? cost = 0;
                                if(product.TryGetProperty("variantsList", out var variants) &&
                                    variants[0].TryGetProperty("localizedLowestPriceCents", out var price) &&
                                    price.TryGetProperty("amountCents", out var cents))
                                {
                                    cost = cents.GetInt32()/100;
                                }

                                decimal retailCost = 0;
                                if(product.TryGetProperty("localizedRetailPriceCents", out var retail) &&
                                    retail.TryGetProperty("amountCents", out var retailPrice))
                                {
                                    retailCost = retailPrice.GetDecimal()/100;
                                }

                                fetchedSneakers.Add(new FetchedSneakerData
                                {
                                    Name = title,
                                    Cost = cost?.ToString() ?? "No cost",
                                    ImageLink = pictureUrl ?? "No image",
                                    Url = url,
                                    SiteId = goatId,
                                    Brand = brand,
                                    silhouette = silhouette
                                });
                                tcs.TrySetResult(true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    }
                
            
            await tcs.Task;

            return fetchedSneakers;
        }

        public async Task<float> GetUsedPrice(string siteId, double size, string condition)
        {
            //await new BrowserFetcher().DownloadAsync();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, // run with visible browser
                SlowMo = 200       // optional: slow actions to look human
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36",
                Locale = "en-US",
                ViewportSize = new ViewportSize { Width = 1400, Height = 800 },
                BypassCSP = true,
                IgnoreHTTPSErrors = true
            });

            var api = $"https://www.goat.com/web-api/v1/products/search?productTemplateId={siteId}&shoeCondition=used&size={size}&sortBy=size&countryCode=US";
            var page = await context.NewPageAsync();
            await page.GotoAsync(api); // product page
            var data = await page.TextContentAsync("pre");
            var goatCondition = condition == "Used" ? "good_condition" : condition == "Badly Damaged" ? "badly_damaged" : "no_original_box";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var readyJson = JsonSerializer.Deserialize<UsedAPIResponse>(data,options);
            var products = readyJson?.Products.Where(p => p.BoxCondition.Equals(goatCondition, StringComparison.OrdinalIgnoreCase))
                .Take(3)
                .Select(p => new UsedItemResult
                {
                    Condition = p.BoxCondition ?? "",
                    imageURL = p.OuterPictureUrl ?? "",
                    Cost = p.PriceCents / 100
                }).ToList();

            if (products == null || products.Count == 0)
                return 0;

            return products.Average(s => s.Cost);
        }

        public async Task<Sneaker> GetSneakerInfoGoat(string url)
        {
            if (string.IsNullOrEmpty(url))
                return new Sneaker();

            Sneaker sneaker = new Sneaker();
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
            await page.GotoAsync(url);

            await page.WaitForSelectorAsync("div[data-qa='facts_window']");
            sneaker.Colorway = await page.Locator("div[data-qa='facts_window']  span:has-text('Colorway') + span").InnerTextAsync();
            sneaker.SKU = await page.Locator("div[data-qa='facts_window']  span:has-text('SKU') + span").InnerTextAsync();
            sneaker.ReleaseDate = await page.Locator("div[data-qa='facts_window']  span:has-text('Release Date') + span").InnerTextAsync();
            sneaker.Creator = await page.Locator("div[data-qa='facts_window']  span:has-text('Designer') + span").InnerTextAsync();
            sneaker.ProductDesc = await page.Locator("div[data-qa='facts_window']  p.WindowItemLongText__Text-sc-1mxjefz-1").InnerTextAsync();
            sneaker.ProductDesc = sneaker.ProductDesc.Trim();
            return sneaker;
        }
    }
}
