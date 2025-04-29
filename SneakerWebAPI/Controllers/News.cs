using HtmlAgilityPack;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System;
using SneakerWebAPI.Services.SneakerService;
using SneakerWebAPI.Services.NewsService;
namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class News : ControllerBase
    {
        private readonly HttpClient _httpclient;
        private readonly INewsService _newsService;

        public News(INewsService newsService)
        {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _newsService = newsService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Newsfeed>>> GetSneakerNews()
        {
            var news = await _newsService.GetNews();
            return Ok(news);
        }

    }
}
