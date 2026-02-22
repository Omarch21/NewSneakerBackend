using Microsoft.AspNetCore.Mvc;
using SneakerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Models.Game;
using SneakerWebAPI.DTOs;
using SneakerWebAPI.Services.GameService;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpclient;
        private readonly IGameService _gameService;
  
        public GameController(DataContext context,HttpClient client, IGameService gameService)
        {
            _context = context;
            _httpclient = client;
            _gameService = gameService;
        }
        [HttpPost]
        public async Task<ActionResult> AddGame(Game game)
        {
            await _gameService.AssignMoreData(game.ResellURL ?? "", game);
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return BadRequest("game was not found.");

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateGame(Game game)
        {
            var cur_game = await _context.Games.FindAsync(game.Id);
            if (cur_game == null)
            {
                return BadRequest("game was not Found");
            }

            cur_game.Name = game.Name;
            cur_game.Console = game.Console;
            cur_game.Genre = game.Genre;
            cur_game.Condition = game.Condition;
            cur_game.Cost = game.Cost;

            return Ok(await _context.SaveChangesAsync());
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchGame(SearchRequest search)
        {
            var games = await _gameService.SearchGamesInSite(search.Search);
            return Ok(games);
        }

        [HttpGet]
        public async Task<ActionResult<List<Game>>> GetGamesForUser(int userId)
        {
            var games = await _context.Games.Where(c => c.UserId == userId).ToListAsync();
            if (games == null)
                return NotFound();
            
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetgameById(int id)
        {
            var game = await _context.Games.FindAsync(id);
            return Ok(game);
        }

        [HttpGet("prices")]
        public async Task<ActionResult<List<GamePrice>>> GetGamePrices(int gameId)
        {
            var prices = await _context.GamePrices.Where(c => c.GameId == gameId).ToListAsync();
            if (prices == null)
                return NotFound();

            return Ok(prices);
        }


    }
}
