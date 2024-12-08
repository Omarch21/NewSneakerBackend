namespace SneakerWebAPI.Services.TokenService
{
    public interface ITokenService
    {
        public string CreateToken (User user);
        public Task<RefreshToken> GenerateRefreshTokenAsync(User user);
        public bool isRefreshTokenValid(User user, string refreshToken);
        public Task<RefreshToken> RotateRefreshTokenAsync(User user);
    }
}
