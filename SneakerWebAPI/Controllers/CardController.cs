using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using SneakerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Services.CardService;
using static System.Net.WebRequestMethods;
using SneakerWebAPI.DTOs;
using SneakerWebAPI.Models.Card;
using SneakerWebAPI.Models;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpclient;
        private readonly ICardService _cardService;

        public CardController(DataContext context, HttpClient client, ICardService cardService)
        {
            _context = context;
            _httpclient = client;
            _cardService = cardService;
        }
        [HttpPost]
        public async Task<ActionResult> AddCard(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null)
                return BadRequest("Card was not found.");

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCard(Card card)
        {
            var cur_card = await _context.Cards.FindAsync(card.Id);
            if (cur_card == null)
            {
                return BadRequest("Card was not Found");
            }

            cur_card.Name = card.Name;
            cur_card.Rarity = card.Rarity;
            cur_card.CardGame = card.CardGame;
            cur_card.CardType = card.CardType;
            cur_card.Set = card.Set;
            cur_card.Condition = card.Condition;
            cur_card.Cost = card.Cost;

            return Ok(await _context.SaveChangesAsync());
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchCard(SearchRequest search)
        {
            var cards = await _cardService.SearchCardsInSite(search.Search);
            return Ok(cards);
        }

        [HttpGet]
        public async Task<ActionResult<List<Card>>> GetCardsForUser(int userId)
        {
            var cards = await _context.Cards.Where(c => c.UserId == userId).ToListAsync();
            if (cards == null)
                return NotFound();

            return Ok(cards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetCardById(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            return Ok(card);
        }

        [HttpGet("prices")]
        public async Task<ActionResult<List<CardPrice>>> GetCardPrices(int cardId)
        {
            var prices = await _context.CardPrices.Where(c => c.CardId == cardId).ToListAsync();
            if (prices == null)
                return NotFound();

            return Ok(prices);
        }
    }
}
