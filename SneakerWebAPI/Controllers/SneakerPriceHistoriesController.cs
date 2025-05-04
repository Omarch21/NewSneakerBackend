using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SneakerWebAPI;
using SneakerWebAPI.Data;
using System.Net.Http;
using SneakerWebAPI.Services.SneakerService;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SneakerPriceHistoriesController : ControllerBase
    {
        private readonly HttpClient _httpclient;
        private readonly DataContext _context;
        private readonly ISneakerService _sneakerservice;

        public SneakerPriceHistoriesController(DataContext context,ISneakerService sneaker)
        {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _context = context;
            _sneakerservice = sneaker;
        }

        // GET: api/SnkrPriceHistories
        [HttpGet]
        public async Task<ActionResult<List<SneakerPriceHistory>>> GetSneakerPrices()
        {
            List<SneakerPriceHistory> pricelist = new List<SneakerPriceHistory>();
            return Ok(pricelist);
        }

        // GET: api/SnkrPriceHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<SneakerPriceHistory>>> GetSneakerPriceHistory(int id)
        {
            var SneakerPriceHistory = await _context.SneakerPrices.Where(o => o.SneakerId == id).ToListAsync();
            if (SneakerPriceHistory == null)
                return NotFound();
    

            return SneakerPriceHistory;
        }

        // PUT: api/SnkrPriceHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        // POST: api/SnkrPriceHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<List<SneakerPriceHistory>>> PostSneakerPriceHistory()
        {
            return Ok(await _sneakerservice.PostSneakerPrices());
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<SneakerPriceHistory>> PostSneakerPriceHistoryById(int id)
        {
            var sneaker = await _context.Sneakers.Where(o => o.Id == id).FirstAsync();
          if(sneaker == null)
            {
                return BadRequest("Was not found");
            }


            string size = sneaker.Size.ToString();
            string source = sneaker.ResellURL;
            float price = await _sneakerservice.GetPrice(size, source);
            sneaker.ResellPrice = price;
            SneakerPriceHistory history = new SneakerPriceHistory()
            {
                Id = sneaker.Id,
                Date = DateTime.Now
            };
            if (history != null)
            {
                history.Price = price;
                
                _context.SneakerPrices.Add(history);
            }



            await _context.SaveChangesAsync();
       
            return Ok(history);
        }
        // DELETE: api/SnkrPriceHistories/5


        private bool SneakerPriceHistoryExists(int id)
        {
            return (_context.SneakerPrices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
