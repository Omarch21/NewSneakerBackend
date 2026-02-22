using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using SneakerWebAPI.Models;
using System.Net.Http;
using System.Web;
using static System.Net.WebRequestMethods;

namespace SneakerWebAPI.Services.NewsService
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _client;
        private readonly string _siteUrl = "https://sneakernews.com/";
        public NewsService(HttpClient client)
        {
            _client = new HttpClient(); 
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        public async Task<List<Newsfeed>> GetNews()
        {
            var response = await _client.GetAsync("https://sneakernews.com/");
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
                    var titleNode = node.SelectSingleNode(".//h3[contains(@class,'popular-stories-list__title')]").SelectSingleNode(".//a");
                    Newsfeed news = new Newsfeed();
                    string title = HttpUtility.HtmlDecode(titleNode.InnerText.Trim());
                    news.description = title;
                    news.title = title;

                    string url = titleNode.GetAttributeValue("href", "");
                    news.url = url;

                    var imageNode = node.SelectSingleNode(".//div[@class='popular-stories-list__image']").SelectSingleNode(".//a").SelectSingleNode(".//img").GetAttributeValue("src", "");
                    news.photoUrl = imageNode;
                    news.id = i++;

                    newslist.Add(news);
                }
                return newslist;
            }
            return newslist;
        }

    }
}
