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
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _context.users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(await _context.users.ToListAsync());
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserSignup request)
        {

            var userlogin = await _context.users.FirstOrDefaultAsync(u => u.Username == request.Username);
            Console.WriteLine(userlogin);
            if (userlogin == null)
            {
                return BadRequest("Account not found");
            }
            if (!VerifyPassword(request.Password, userlogin.PasswordHash, userlogin.PasswordSalt))
            {
                return BadRequest("Wrong password or Email");
            }

            string token = CreateToken(userlogin);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);
            return Ok(token);
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid refresh token.");
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired");
            }
            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);
            return Ok(token);


        }
        [HttpPost("logout")]
        public async Task<ActionResult<string>> Logout()
        {
            // Invalidate the user's refresh token (set it to null in the database).
            var user = await _context.users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (user != null)
            {
                user.RefreshToken = null;
                await _context.SaveChangesAsync();
            }

            // Clear the refresh token cookie from the response.
            Response.Cookies.Delete("refreshToken");

            // Optionally, you can also invalidate the user's access token on the client-side.
            return Ok(user);
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
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
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
