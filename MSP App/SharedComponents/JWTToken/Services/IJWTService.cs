using SharedComponents.JWTToken.Entities;

namespace SharedComponents.JWTToken.Services
{
    public interface IJWTService
    {
        public JWTSettings JWTSettings { get; }
        public Task<string> GenerateTokenAsync(string userId, string userName, ICollection<string> roles);
        public Task<string> GenerateRefreshTokenAsync();
        public Task<IDictionary<string, string>> ReadClaimsAsync(string token);
        public Task<ICollection<string>> GetRolesFromTokenAsync(string token);
        public Task<string> GetUsernameFromTokenAsync(string token);
        public Task<string> GetUserIdFromTokenAsync(string token);
        public Task<DateTime?> GetTokenExpiryAsync(string token);
    }
}
