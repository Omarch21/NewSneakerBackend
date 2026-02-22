using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SneakerWebAPI.Services.UserService;
using SneakerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Services.TokenService;
using SneakerWebAPI.DTOs;
using PuppeteerSharp;
using System.Text.RegularExpressions;
using SneakerWebAPI.Models;
using SneakerWebAPI.Models.Sneaker;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly HttpClient _httpClient;
        public UserController(IConfiguration configuration, IUserService userService, DataContext context, ITokenService tokenService, HttpClient httpClient)
        {
            _configuration = configuration;
            _userService = userService;
            _context = context;
            _tokenService = tokenService;
            _httpClient = httpClient;
        }


        [HttpGet, Authorize]
        public ActionResult<User> GetMe()
        {
            var username = _userService.GetMyName();
            User currentUser;
            if (username != null)
            {
                currentUser = _context.users.SingleOrDefault(u => u.Username == username);
                User newuser = new User();
                newuser.Username = currentUser.Username;
                newuser.Id = currentUser.Id;
                newuser.Email = currentUser.Email;
                newuser.PhoneNumber = currentUser.PhoneNumber;
                return Ok(newuser);
            }
            return Ok("No User Found");
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserSignup request)
        {
            await _userService.RegisterUserAsync(request);
            return Ok("User Registered Successfully");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest request)
        {

            var userlogin = await _userService.GetUserByUsernameAsync(request.Username);
            if (userlogin == null || !_userService.VerifyPassword(request.Password, userlogin.PasswordHash, userlogin.PasswordSalt))
            {
                return BadRequest("Wrong Email or Password");
            }

            user = userlogin;

            var accessToken = _tokenService.CreateToken(userlogin);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            Response.Cookies.Append("refreshToken", refreshToken.Token,
               new CookieOptions
               {
                   HttpOnly = true,
                   IsEssential = true,
                   Secure = true,
                   SameSite = SameSiteMode.None
               });

            return Ok(accessToken);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Invalid refresh token.");
            }

            var user = await _context.users.FirstOrDefaultAsync(c => c.RefreshToken == refreshToken);
            if (user == null || user.TokenExpires < DateTime.UtcNow)
            {
                return BadRequest("Invalid or Expired refresh token.");
            }

            var token = _tokenService.CreateToken(user);

            return Ok(token);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUser()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await _context.users.FirstOrDefaultAsync(c => c.RefreshToken == refreshToken);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.TokenExpires = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            });

            return Ok(new { message = "Logged out successfully", Success = true });
        }

        private async Task<User> GetAuthenticatedUser()
        {
            var token = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return null;

            var user = await _context.users.FirstOrDefaultAsync(c => c.RefreshToken == token);
            if (user == null || user.TokenExpires < DateTime.Now)
                return null;

            return user;
        }


        [HttpGet("isAuthenticated")]
        public async Task<ActionResult<bool>> IsAuthenticated()
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
                return Unauthorized("Invalid or expired token");

            return Ok(true);
        }

        [Authorize]
        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetUserDate()
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
                return Unauthorized("Not authenticated");

            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult<User>> UpdateUser(User user)
        {
            var newuser = await _context.users.FindAsync(user.Id);
            if (newuser == null)
            {
                return BadRequest("User not found");
            }
            await _userService.UpdateUserAsync(newuser);
            return Ok(newuser);

        }

        [HttpPost("Sneaker")]
        public async Task<IActionResult> Get1(SearchRequest request)
        {
            string searchQuery = request.Search;
            string searchUrl = $"https://www.flightclub.com/catalogsearch/result?query={Uri.EscapeDataString(searchQuery)}";

            //await new BrowserFetcher().DownloadAsync();
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
            {
                var page = await browser.NewPageAsync();
                await page.GoToAsync(searchUrl);
                var a = await page.GetContentAsync();
                Console.WriteLine(a);
                await page.WaitForSelectorAsync(".LinkCTA__StyledLink-fc-web__sc-1wc5x2x-0");
                var items = await page.EvaluateFunctionAsync<List<FetchedSneakerData>>(@"
                                () => {
                                const elements = Array.from(document.querySelectorAll('.LinkCTA__StyledLink-fc-web__sc-1wc5x2x-0'));
                                return elements.slice(0,3).map(el=>({
                                link: el.href || 'No link',
                                imageLink: el.querySelector('img')?.src || 'No image',
                                cost: el.querySelector('[data-qa=""ProductItemPrice""]')?.textContent.trim() || 'No cost',
                                name: el.querySelector('[data-qa=""ProductItemTitle""]')?.textContent.trim() || 'No name',
                                brand: el.querySelector('[data-qa=""ProductItemBrandName""]')?.textContent.trim() || 'No brand'
                                }));}");

                string pattern = @"'([^']*)'";
                foreach (var item in items)
                {
                    string[] jordanNumber = item.Name.Split(' ');
                    int jordanNumberIndex = Array.IndexOf(jordanNumber, "Jordan");
                    item.JordanNumber = (jordanNumberIndex >= 0) ? int.Parse(jordanNumber[++jordanNumberIndex]) : 0;
                    Match match = Regex.Match(item.Name, pattern);
                    item.Nickname = match.Groups[1].Value;
                }
                return Ok(items);
            }

        }
        [HttpPost("info")]
        public async Task<IActionResult> Get1More(Sneaker request)
        {
            Console.WriteLine(request);
            return Ok();
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllItems(int userId)
        {
            List<BaseItem> allItems = new();

            var games = await _context.Games.Where(c => c.UserId == userId).ToListAsync();
            var sneakers = await _context.Sneakers.Where(c => c.UserId == userId).ToListAsync();
            var cards = await _context.Cards.Where(c => c.UserId == userId).ToListAsync();
            var pops = await _context.FunkoPops.Where(c => c.UserId == userId).ToListAsync();

            var updatedSneakers = sneakers.Select(s =>
            {
                s.Name = $"{s.Brand} {s.Silhouette} {s.Name}";
                return s;
            }).ToList();

            var updatedGames = games.Select(s =>
            {
                s.Name = $"{s.Console} {s.Name}";
                return s;
            }).ToList();

            var updatedCards = cards.Select(s =>
            {
                s.Name = $"{s.CardGame} {s.Name}";
                return s;
            }).ToList();

            allItems.AddRange(updatedSneakers);
            allItems.AddRange(updatedGames);
            allItems.AddRange(updatedCards);
            allItems.AddRange(pops);
            return Ok(allItems);
        }

        [HttpPost("sold")]
        public async Task<IActionResult> ItemSold(ItemSoldRequest request)
        {
            switch (request.ItemType)
            {
                case "Sneaker":
                    var sneaker = await _context.Sneakers.FindAsync(request.ItemId);
                    if (sneaker == null)
                        return NotFound();
                    sneaker.Sold = true;
                    break;
                case "Game":
                    var game = await _context.Games.FindAsync(request.ItemId);
                    if (game == null)
                        return NotFound();
                    game.Sold = true;
                    break;
                case "Card":
                    var card = await _context.Cards.FindAsync(request.ItemId);
                    if (card == null)
                        return NotFound();
                    card.Sold = true;
                    break;
                case "FunkoPop":
                    var funkoPop = await _context.FunkoPops.FindAsync(request.ItemId);
                    if (funkoPop == null)
                        return NotFound();
                    funkoPop.Sold = true;
                    break;
                default:
                    return BadRequest($"Invalid item type: {request.ItemType}");
            }

            await _context.SaveChangesAsync();
            return Ok(true);
        }
    }
}
