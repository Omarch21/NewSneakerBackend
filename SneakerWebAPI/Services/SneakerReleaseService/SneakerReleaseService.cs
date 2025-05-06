using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using SneakerWebAPI.DTOs;
using System.Net.Http;
using System.Web;
using static System.Net.WebRequestMethods;

namespace SneakerWebAPI.Services.SneakerReleaseService
{
    public class SneakerReleaseService : ISneakerReleaseService
    {
        private readonly HttpClient _client;
        private readonly string _url = "https://www.soleretriever.com/sneaker-release-dates/jordan";
        public SneakerReleaseService(HttpClient client)
        {
            _client = new HttpClient(); 
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        public async Task<List<SneakerReleaseResponse>> GetReleases()
        {
            var response = await _client.GetAsync(_url);
            string siteHtml = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(siteHtml);

            List<SneakerReleaseResponse> sneakerReleasesList = new List<SneakerReleaseResponse>();
            var sneakers = document.DocumentNode.SelectNodes("//a[contains(@class,'bg-[var(--light-bg-color)]')]")?.Take(5);
            if(sneakers != null)
                foreach(var sneaker in sneakers)
                {
                    SneakerReleaseResponse sneakerRelease = new SneakerReleaseResponse();
                    var src = sneaker.SelectSingleNode(".//img[contains(@class, 'thumbnail')]").GetAttributeValue("srcset", "");
                    if (!string.IsNullOrEmpty(src))
                    {
                        sneakerRelease.ImageURL = src.Split(',')[0].Trim().Split(' ')[0];
                    }
                    var moreInfoSrc = sneaker.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(moreInfoSrc))
                    {
                        sneakerRelease.MoreInfoSource = "https://www.soleretriever.com/" + moreInfoSrc;
                    }
                    var name = sneaker.SelectSingleNode(".//p[contains(@class, 'sm:h-[3em] mr-4')]").InnerText.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {   
                        sneakerRelease.Name = HttpUtility.HtmlDecode(name);
                    }
                    var date = sneaker.SelectSingleNode(".//p[contains(@class, 'dark:mix-blend-overlay text-black/50 dark:text-gray-100')]").InnerText.Trim();
                    if (!string.IsNullOrEmpty(date))
                    {
                        sneakerRelease.ReleaseDate = date;
                    }
                    var cost = sneaker.SelectSingleNode(".//*[contains(text(), '$')]").InnerText.Trim().Replace("$","");
                    if (!string.IsNullOrEmpty(cost)) {
                        sneakerRelease.Retail = int.Parse(cost);
                    }
                    var productCode = sneaker.SelectSingleNode(".//p[contains(@class, 'dark:mix-blend-overlay text-black/50 dark:text-white')]").InnerText.Trim();
                    if (!string.IsNullOrEmpty(productCode))
                    {
                        sneakerRelease.ProductCode = productCode;
                    }
                    sneakerReleasesList.Add(sneakerRelease);
                }

            return sneakerReleasesList;
        }
       

    }
}
