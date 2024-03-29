using HtmlAgilityPack;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System;
namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class News : ControllerBase
    {
        private readonly HttpClient _httpclient;

        public News()
        {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        [HttpGet]
        public async Task<ActionResult<List<Newsfeed>>> GetSneakerNews()
            {
            var response = await _httpclient.GetAsync("https://sneakernews.com/");
            string shoe = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(shoe);
 
            List<Newsfeed> newslist = new List<Newsfeed>();
            var div = document.DocumentNode.SelectNodes("//div[contains(@class,'popular-stories-list__item')]");
            if (div != null)
            {
                int i = 0;
                foreach (HtmlNode node in div)
                {
                    var title = node.SelectSingleNode(".//h4[contains(@class,'popular-stories-list__title')]");
                    Newsfeed news = new Newsfeed();
                    string a = title.InnerText.Trim();
                    news.description = HttpUtility.HtmlDecode(a);
                    var anchor = node.SelectSingleNode(".//a[@class='post-title']");
                    string anchorUrl = title?.GetAttributeValue("href", "");
                    news.url = anchorUrl;
                    var anchor2 = node.SelectSingleNode(".//div[@class='popular-stories-list__image']");
                    var anchor3 = anchor2.SelectSingleNode(".//img");

                    string anchorUrl2 = anchor3?.GetAttributeValue("src", "");
                    news.photoUrl = anchorUrl2;
                    //string words = node.InnerText.Trim();
                    news.id = i;
                    i++;
                    newslist.Add(news);
                }
                return Ok(newslist);
            }
            return Ok(newslist);
    }

    }
}
