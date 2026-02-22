using SneakerWebAPI.DTOs;
using SneakerWebAPI.Models;

namespace SneakerWebAPI.Services.UserService
{
    public interface IUserService
    {
        public string GetMyName();
        public Task RegisterUserAsync(UserSignup request);
        public Task<User?> UpdateUserAsync(User user);
        public Task<User> GetUserByUsernameAsync(string username);
        public bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
  
    }
}
