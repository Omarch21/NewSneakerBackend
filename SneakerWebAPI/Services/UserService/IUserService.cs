using SneakerWebAPI.DTOs;

namespace SneakerWebAPI.Services.UserService
{
    public interface IUserService
    {
        public string GetMyName();
        public Task RegisterUser(UserSignup request);
        public Task LoginUser(LoginRequest request);
        public Task UpdateUserAsync(User user);
  
    }
}
