using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using SneakerWebAPI.DTOs;
using System.Text.RegularExpressions;
using System.Web;

namespace SneakerWebAPI.Services.SneakerReleaseService
{
    public class SneakerReleaseService : ISneakerReleaseService
    {
        private readonly HttpClient _client;
        private readonly string _url = "https://www.nicekicks.com/air-jordan-release-dates/?nk=upcoming";
        private readonly string _baseUrl = "https://www.kicksonfire.com/";
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
            var sneakers = document.DocumentNode.SelectNodes("//div[@class='release-date__wrapper']")?.Take(5);
            if (sneakers != null)
                foreach (var sneaker in sneakers)
                {
                    SneakerReleaseResponse sneakerRelease = new SneakerReleaseResponse();
                    var src = sneaker.SelectSingleNode(".//img[@class='attachment-full size-full']").GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(src))
                    {
                        sneakerRelease.ImageURL = src;
                    }
                    var moreInfoSrc = sneaker.SelectSingleNode(".//a").GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(moreInfoSrc))
                    {
                        sneakerRelease.MoreInfoSource = moreInfoSrc;
                    }
                    var name = sneaker.SelectSingleNode(".//div[@class='release-date__title']//a").InnerText.Trim();

                    if (!string.IsNullOrEmpty(name))
                    {
                        sneakerRelease.Name = HttpUtility.HtmlDecode(name);
                    }
                    var info = sneaker.SelectSingleNode(".//div[@class='release-block__content']//p").InnerText;
                    if (!string.IsNullOrEmpty(info))
                    {
                        var releaseDate = ExtractValue(info, "Release Date:");
                        sneakerRelease.ReleaseDate = releaseDate;

                        var cost = ExtractValue(info, "Price:");
                        sneakerRelease.Retail = int.Parse(cost);

                        var productCode = ExtractValue(info, "Style #:");
                        sneakerRelease.ProductCode = productCode;
                    }
                    sneakerReleasesList.Add(sneakerRelease);
                }

            return sneakerReleasesList;
        }

        private string ExtractValue(string input, string key)
        {
            var pattern = key switch
            {
                "Style #:" => @"Style #:\s*(.*?)\s*(?=Release Date:|Price:|$)",
                "Release Date:" => @"Release Date:\s*(.*?)\s*(?=Price:|Style #:|$)",
                "Price:" => @"Price:\s*\$?([0-9.,]+)",
                _ => throw new ArgumentException("Unknown key")
            };

            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }
    }
    
}
/* "https://www.kicksonfire.com/air-jordan-release-dates?sort=upcoming";
 * public async Task<List<SneakerReleaseResponse>> GetReleases()
        {
            var response = await _client.GetAsync(_url);
            string siteHtml = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(siteHtml);

            List<SneakerReleaseResponse> sneakerReleasesList = new List<SneakerReleaseResponse>();
            var sneakers = document.DocumentNode.SelectNodes("//a[@class='release-item']")?.Take(5);
            if (sneakers != null)
                foreach (var sneaker in sneakers)
                {
                    SneakerReleaseResponse sneakerRelease = new SneakerReleaseResponse();
                    var src = sneaker.SelectSingleNode(".//img[@class='img-responsive']").GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(src))
                    {
                        sneakerRelease.ImageURL = src;
                    }
                    var moreInfoSrc = sneaker.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(moreInfoSrc))
                    {
                        sneakerRelease.MoreInfoSource = _baseUrl + moreInfoSrc;
                    }
                    var name = sneaker.SelectSingleNode(".//span[@class='release-item-title']").InnerText.Trim();

                    if (!string.IsNullOrEmpty(name))
                    {
                        sneakerRelease.Name = HttpUtility.HtmlDecode(name);
                    }
                    var date = sneaker.SelectSingleNode(".//span[@class='release-price-from']").InnerText.Trim();
                    if (!string.IsNullOrEmpty(date))
                    {
                        sneakerRelease.ReleaseDate = date;
                    }
                    var cost = sneaker.SelectSingleNode(".//*[contains(text(), '$')]").InnerText.Trim().Replace("$", "");
                    if (!string.IsNullOrEmpty(cost))
                    {
                        sneakerRelease.Retail = int.Parse(cost);
                    }
                    var productCode = sneaker.SelectSingleNode(".//p[contains(@class, 'dark:mix-blend-overlay text-black/50 dark:text-white')]")?.InnerText?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(productCode))
                    {
                        sneakerRelease.ProductCode = productCode;
                    }
                    sneakerReleasesList.Add(sneakerRelease);
                }

            return sneakerReleasesList;
        }*/