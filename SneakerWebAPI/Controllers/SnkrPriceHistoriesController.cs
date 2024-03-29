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
    public class SnkrPriceHistoriesController : ControllerBase
    {
        private readonly HttpClient _httpclient;
        private readonly DataContext _context;
        private readonly ISneakerService _sneakerservice;

        public SnkrPriceHistoriesController(DataContext context,ISneakerService sneaker)
        {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _context = context;
            _sneakerservice = sneaker;
        }

        // GET: api/SnkrPriceHistories
        [HttpGet]
        public async Task<ActionResult<List<SnkrPriceHistory>>> GetSnkrPrices()
        {
            var shoes = await _context.Sneakers.ToListAsync();
            List<SnkrPriceHistory> pricelist = new List<SnkrPriceHistory>();
            if ((shoes == null))
            {
                return BadRequest("Sneaker was not Found");
            }
            foreach (var shoe1 in shoes)
            {

                string size = shoe1.Size.ToString();
                string source = shoe1.ResellURL;
                float price = _sneakerservice.GetPrice(size,source);
                shoe1.ResellPrice = price;
                SnkrPriceHistory history = new SnkrPriceHistory()
                {
                    Id = shoe1.Id,
                    Date = DateTime.Now
                };
                if(history != null)
                {
                    history.Price = price;
                    pricelist.Add(history);
                    _context.SnkrPrices.Add(history);
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                Console.WriteLine(Error.Message);
            }
            return Ok(pricelist);
        }

        // GET: api/SnkrPriceHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<SnkrPriceHistory>>> GetSnkrPriceHistory(int id)
        {
          if (_context.SnkrPrices == null)
          {
              return NotFound();
          }
            var snkrPriceHistory = await _context.SnkrPrices.Where(o => o.SneakerId == id).ToListAsync();
            Console.WriteLine(snkrPriceHistory);
            if (snkrPriceHistory == null)
            {
                return NotFound();
            }

            return snkrPriceHistory;
        }

        // PUT: api/SnkrPriceHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        // POST: api/SnkrPriceHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SnkrPriceHistory>> PostSnkrPriceHistory()
        {
            var shoes = await _context.Sneakers.ToListAsync();
            List<SnkrPriceHistory> pricelist = new List<SnkrPriceHistory>();
            if ((shoes == null))
            {
                return BadRequest("Sneaker was not Found");
            }
            foreach (var shoe1 in shoes)
            {

                string size = shoe1.Size.ToString();
                string source = shoe1.ResellURL;
                float price = _sneakerservice.GetPrice(size, source);
                shoe1.ResellPrice = price;
                SnkrPriceHistory history = new SnkrPriceHistory()
                {
                    Id = shoe1.Id,
                    Date = DateTime.Now
                };
                if (history != null)
                {
                    history.Price = price;
                    pricelist.Add(history);
                    _context.SnkrPrices.Add(history);
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                Console.WriteLine(Error.Message);
            }
            return Ok(pricelist);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<SnkrPriceHistory>> PostSnkrPriceHistoryById(int id)
        {
            var sneaker = await _context.Sneakers.Where(o => o.Id == id).FirstAsync();
          if(sneaker == null)
            {
                return BadRequest("Was not found");
            }


            string size = sneaker.Size.ToString();
            string source = sneaker.ResellURL;
            float price = _sneakerservice.GetPrice(size, source);
            sneaker.ResellPrice = price;
            SnkrPriceHistory history = new SnkrPriceHistory()
            {
                Id = sneaker.Id,
                Date = DateTime.Now
            };
            if (history != null)
            {
                history.Price = price;
                
                _context.SnkrPrices.Add(history);
            }



            await _context.SaveChangesAsync();
       
            return Ok(history);
        }
        // DELETE: api/SnkrPriceHistories/5


        private bool SnkrPriceHistoryExists(int id)
        {
            return (_context.SnkrPrices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
