using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using SneakerWebAPI.Services.UserService;
using SneakerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI;

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
        public UserController(IConfiguration configuration, IUserService userService, DataContext context)
        {
            _configuration = configuration;
            _userService = userService;
            _context = context;
        }


        [HttpGet, Authorize]
        public ActionResult<User> GetMe()
        {
            var username = _userService.GetMyName();
            User currentUser;
            if (username != null)
            {
                currentUser = _context.users.SingleOrDefault(u => u.Username == username);
                User newuser = new SneakerWebAPI.User();
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
            await _userService.RegisterUser(request);
            return Ok("User Registered Successfully");
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserSignup request)
        {

            var userlogin = await _context.users.FirstOrDefaultAsync(u => u.Username == request.Username);
            Console.WriteLine(userlogin);
            if (userlogin == null || !VerifyPassword(request.Password, userlogin.PasswordHash, userlogin.PasswordSalt))
            {
                return BadRequest("Wrong Email or Password");
            }

            user = userlogin;
            string token = CreateToken(userlogin);

            var refreshToken = GenerateRefreshToken();
            SetRefreshTokenAsync(refreshToken);
            return Ok(token);
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("Invalid refresh token.");
            }

            var user = await _context.users.FirstOrDefaultAsync(c => c.RefreshToken == refreshToken);
            if(user == null || user.TokenExpires < DateTime.UtcNow)
            {
                return Unauthorized("INvalid or Expired refresh token.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();


            SetRefreshTokenAsync(newRefreshToken);
            return Ok(token);


        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now

            };
            return refreshToken;
        }
        private async Task SetRefreshTokenAsync(RefreshToken newRefreshToken)
        {

            Response.Cookies.Append("refreshToken", newRefreshToken.Token,
               new CookieOptions
               {
                   Expires = DateTimeOffset.UtcNow.AddDays(7),
                   HttpOnly = true,
                   IsEssential = true,
                   Secure = true,
                   SameSite = SameSiteMode.None
               });

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
            _context.users.Update(user);
            await _context.SaveChangesAsync();
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Username)

         };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private async Task<User> GetAuthenticatedUser()
        {
            var token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
                return null;

            var user = await _context.users.FirstOrDefaultAsync(c => c.RefreshToken == token);
            if (user == null || user.TokenExpires < DateTime.Now)
                return null;

            return user;
        }
        [HttpGet("isAuthenticated")]
        public async Task<IActionResult> IsAuthenticated()
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
                return Unauthorized("Invalid or expired token");

            return Ok(true);
        }

        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetUserDate()
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
                return Unauthorized("Not authenticated");

            return Ok(user);
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        [HttpPut]
        public async Task<ActionResult<User>> UpdateUser(User user)
        {
            var newuser = await _context.users.FindAsync(user.Id);
            if(newuser == null)
            {
                return BadRequest("User not found");
            }
            Console.WriteLine(user.PhoneNumber);
            newuser.Email = user.Email;
            newuser.PhoneNumber = user.PhoneNumber;
            await _context.SaveChangesAsync();
            return Ok(newuser);

        }
    }
}
