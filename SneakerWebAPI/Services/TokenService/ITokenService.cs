namespace SneakerWebAPI.Services.TokenService
{
    public interface ITokenService
    {
        public string CreateToken (User user);
        public RefreshToken GenerateRefreshToken(User user);
    }
}
