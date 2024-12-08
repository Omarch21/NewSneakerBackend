using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SneakerWebAPI.Data;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SneakerController : ControllerBase
    {
        private readonly HttpClient _httpclient;
        private readonly DataContext _context;

        public SneakerController(DataContext context) {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<List<Sneaker>>> GetSneakers()
        {
            return Ok(await _context.Sneakers.ToListAsync());
        }
       [HttpGet("{id}")]
       public async Task<ActionResult<Sneaker>> GetSneakerById(int id)
        {
            var show = await _context.Sneakers.FindAsync(id);
            if (show == null)
            {
                return NotFound();
            }
            return Ok(await _context.Sneakers.FindAsync(id));
            
        }
        [HttpPost]
        public async Task<ActionResult<List<Sneaker>>> AddSneaker(Sneaker sneaker)
        {
           




               string searchQuery = "mens air jordan 11 retro bred";
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            IWebDriver driver = new ChromeDriver(chromeOptions);

            try
            {
 


                // Navigate to the desired URL with the dynamic search query
                string searchUrl = $"https://www.flightclub.com/catalogsearch/result?query={Uri.EscapeDataString(searchQuery)}";
                driver.Navigate().GoToUrl(searchUrl);

                // Wait for the page to load (you may need to adjust the wait time)
                System.Threading.Thread.Sleep(4000);

                // Execute JavaScript to get the full page source
                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                string fullHtmlContent = (string)jsExecutor.ExecuteScript("return document.documentElement.outerHTML;");

                // Print the HTML content to the console
               // Console.WriteLine(fullHtmlContent);

                // Find the image element (if it exists)
                IWebElement divElement = driver.FindElement(By.CssSelector("a[data-qa='ProductItemsUrl']"));
                string url = divElement.GetAttribute("href");
                // Find the image element within the div
                IWebElement imageElement = divElement.FindElement(By.TagName("img"));

                // Get the source (URL) of the image
                string imageUrl = imageElement.GetAttribute("src");

                // Print the image URL to the console
                Console.WriteLine("Image URL: " + imageUrl + url);
                sneaker.PhotoURL = imageUrl;
                sneaker.ResellURL = url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Close the WebDriver instance
                driver.Quit();
            }
            if (sneaker.ResellURL != null)
            {
                string source = sneaker.ResellURL;
                var response = await _httpclient.GetAsync(source);
                string shoe = await response.Content.ReadAsStringAsync();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(shoe);
                var size = sneaker.Size;
                HtmlNode ProductInfoDate = document.DocumentNode.SelectSingleNode("//span[@data-qa='ProductReleaseDate']");
                HtmlNode ProductInfoSKU = document.DocumentNode.SelectSingleNode("//span[@data-qa='ProductSku']");
                HtmlNode ProductInfoColorway = document.DocumentNode.SelectSingleNode("//span[@data-qa='ProductColorway']");

                HtmlNode ProductDesc = document.DocumentNode.SelectSingleNode("//div[@data-qa='ProductDescriptionText']");
                string pricefinder = "//script[contains(.,'\"size\":" + size + "')][contains(.,'\"price\":')]";
                HtmlNode scriptNode = document.DocumentNode.SelectSingleNode(pricefinder);
              

                if (scriptNode != null)
                {
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
                            sneaker.ResellPrice = float.Parse(size8_5Price);
                            
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
                if (ProductInfoDate != null)
                {
                    string info = ProductInfoDate.InnerText;
                    string[] date = info.Split(':');
                    string onlydate = date[1].Trim();
                    sneaker.ReleaseDate = onlydate;
                }
                if (ProductInfoSKU != null)
                {
                    string info = ProductInfoSKU.InnerText;
                    string[] sku = info.Split(':');
                    string onlysku = sku[1].Trim();
                    sneaker.SKU = onlysku;
                }
                if (ProductInfoColorway != null)
                {
                    string info = ProductInfoColorway.InnerText;
                    string[] colorway = info.Split(':');
                    string onlycolorway = colorway[1].Trim();
                    sneaker.Colorway = onlycolorway;
                }
                if (ProductDesc != null)
                {
                    HtmlNodeCollection info = ProductDesc.SelectNodes(".//p");
                    string info2 = ProductDesc.InnerText;
                    sneaker.ProductDesc = info2;
                }
            }
            _context.Sneakers.Add(sneaker);
            await _context.SaveChangesAsync();
          
            return Ok(await _context.Sneakers.ToListAsync());
        }

        [HttpPut]
        public async Task<ActionResult<List<Sneaker>>> UpdateSneaker(Sneaker sneaker)
        {
            var newsneaker = await _context.Sneakers.FindAsync(sneaker.Id);
            if ((newsneaker == null))
            {
                return BadRequest("Sneaker was not Found");
            }

            newsneaker.Brand = sneaker.Brand;
            newsneaker.Nickname = sneaker.Nickname;
            newsneaker.Silhouette = sneaker.Silhouette;
            newsneaker.Retail = sneaker.Retail;
            newsneaker.PhotoURL = sneaker.PhotoURL;
            newsneaker.ResellPrice = sneaker.ResellPrice;
            await _context.SaveChangesAsync();

            return Ok(await _context.Sneakers.ToListAsync());

        }
        [HttpDelete("{id}")]
    public async Task<ActionResult<List<Sneaker>>> DeleteSneaker(int id)
        {

            var sneaker = await _context.Sneakers.FindAsync(id);
            if ((sneaker == null))
            {
                return BadRequest("Sneaker was not Found");
            }

            _context.Sneakers.Remove(sneaker);
            await _context.SaveChangesAsync();

            return Ok(await _context.Sneakers.ToListAsync());

        }
    }
}
