using Microsoft.AspNetCore.Mvc;
using SneakerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Models.FunkoPop;
using SneakerWebAPI.DTOs;
using SneakerWebAPI.Services.FunkoPopService;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunkoPopController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpclient;
        private readonly IFunkoPopService _FunkoPopService;
  
        public FunkoPopController(DataContext context,HttpClient client, IFunkoPopService FunkoPopService)
        {
            _context = context;
            _httpclient = client;
            _FunkoPopService = FunkoPopService;
        }
        [HttpPost]
        public async Task<ActionResult> AddFunkoPop(FunkoPop funkoPop)
        {
            _context.FunkoPops.Add(funkoPop);
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteFunkoPop(int id)
        {
            var funkoPop = await _context.FunkoPops.FindAsync(id);
            if (funkoPop == null)
                return BadRequest("Funko Pop was not found.");

            _context.FunkoPops.Remove(funkoPop);
            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateFunkoPop(FunkoPop FunkoPop)
        {
            var cur_FunkoPop = await _context.FunkoPops.FindAsync(FunkoPop.Id);
            if (cur_FunkoPop == null)
            {
                return BadRequest("FunkoPop was not Found");
            }

            cur_FunkoPop.Name = FunkoPop.Name;
            cur_FunkoPop.Condition = FunkoPop.Condition;
            cur_FunkoPop.Cost = FunkoPop.Cost;

            return Ok(await _context.SaveChangesAsync());
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchFunkoPop(SearchRequest search)
        {
            var FunkoPops = await _FunkoPopService.SearchFunkoPopsInSite(search.Search);
            return Ok(FunkoPops);
        }

        [HttpGet]
        public async Task<ActionResult<List<FunkoPop>>> GetFunkoPopsForUser(int userId)
        {
            var funkoPops = await _context.FunkoPops.Where(c => c.UserId == userId).ToListAsync();
            if (funkoPops == null)
                return NotFound();
            
            return Ok(funkoPops);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FunkoPop>> GetFunkoPopById(int id)
        {
            var funkoPop = await _context.FunkoPops.FindAsync(id);
            return Ok(funkoPop);
        }

        [HttpGet("prices")]
        public async Task<ActionResult<List<FunkoPopPrice>>> GetFunkoPopPrices(int FunkoPopId)
        {
            var prices = await _context.FunkoPopPrices.Where(c => c.FunkoPopId == FunkoPopId).ToListAsync();
            if (prices == null)
                return NotFound();

            return Ok(prices);
        }


    }
}
