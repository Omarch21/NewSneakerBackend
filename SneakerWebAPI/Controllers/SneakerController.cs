using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SneakerWebAPI.Data;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PuppeteerSharp;
using SneakerWebAPI.Services.SneakerService;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SneakerController : ControllerBase
    {
        private readonly HttpClient _httpclient;
        private readonly DataContext _context;
        private readonly ISneakerService _sneakerService;

        public SneakerController(DataContext context, ISneakerService sneakerService) {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _context = context;
            _sneakerService = sneakerService;
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
        [HttpPost("GetSneakersByUserId")]
        public async Task<ActionResult<List<Sneaker>>> GetSneakerByUserId([FromBody]int userId)
        {
            var sneakers = await _context.Sneakers.Where(a => a.UserID == userId).ToListAsync();
            if (sneakers == null)
            {
                return NotFound();
            }
            return Ok(sneakers);
        }

        [HttpPost]
        public async Task<ActionResult<List<Sneaker>>> AddSneaker(Sneaker sneaker)
        {
            var newSneaker = await _sneakerService.GetSneakerInfo(sneaker.ResellURL ?? "");
            sneaker.SKU = newSneaker.SKU;
            sneaker.ProductDesc = newSneaker.ProductDesc;
            sneaker.Colorway = newSneaker.Colorway;
            sneaker.ReleaseDate = newSneaker.ReleaseDate;
            sneaker.ResellPrice = await _sneakerService.GetPrice(sneaker.Size.ToString(), sneaker.ResellURL);

            _context.Sneakers.Add(sneaker);
            await _context.SaveChangesAsync();
            return Ok(true);

        }

        [HttpPut]
        public async Task<ActionResult<List<Sneaker>>> UpdateSneaker(Sneaker sneaker)
        {
            var cur_sneaker = await _context.Sneakers.FindAsync(sneaker.Id);
            if ((cur_sneaker == null))
            {
                return BadRequest("Sneaker was not Found");
            }

            cur_sneaker.Brand = sneaker.Brand;
            cur_sneaker.Nickname = sneaker.Nickname;
            cur_sneaker.Silhouette = sneaker.Silhouette;
            cur_sneaker.Retail = sneaker.Retail;
            cur_sneaker.PhotoURL = sneaker.PhotoURL ?? cur_sneaker.PhotoURL;
            cur_sneaker.ResellPrice = sneaker.ResellPrice;
            cur_sneaker.Size = sneaker.Size;
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
