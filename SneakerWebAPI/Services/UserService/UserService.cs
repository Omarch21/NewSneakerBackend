
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Data;
using SneakerWebAPI.DTOs;
using SneakerWebAPI.Models;
using SneakerWebAPI.Services.TokenService;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SneakerWebAPI.Services.UserService
{
    public class UserService : IUserService
    {
        public static User user = new User();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _context;

        public UserService(IHttpContextAccessor httpContextAccessor, DataContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public string GetMyName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }

        public async Task RegisterUserAsync(UserSignup request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Username;

            _context.users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> UpdateUserAsync(User updatedUser)
        {
            var user = await _context.users.FirstOrDefaultAsync(c => c.Username == updatedUser.Username);
            if (user == null)
                return null;

            user.Email = updatedUser.Email ?? user.Email;
            user.RefreshToken = updatedUser.RefreshToken ?? user.RefreshToken;
            user.TokenExpires = updatedUser.TokenExpires;
            user.TokenCreated = updatedUser.TokenCreated;

            _context.users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.users.FirstOrDefaultAsync(u => u.Username == username);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

    }
}