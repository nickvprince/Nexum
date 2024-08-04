using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.JWTToken.Entities;
using SharedComponents.Utilities;
using System.Security.Claims;
using System.Text;

namespace SharedComponents.JWTToken.Services
{
    public class JWTService : IJWTService
    {
        private readonly JWTSettings? _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JWTService(IConfiguration config)
        {
            _jwtSettings = config.GetSection("JWTSettings").Get<JWTSettings>();

            var key = Encoding.ASCII.GetBytes(SecurityUtilities.PadKey(_jwtSettings.SecretKey, 32));

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = _jwtSettings.Audience,
                ValidIssuer = _jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        public JWTSettings JWTSettings => _jwtSettings ?? throw new NullReferenceException("JWTSettings is null");

        public async Task<string> GenerateTokenAsync(string userId, string userName, ICollection<string> roles)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecurityUtilities.PadKey(_jwtSettings.SecretKey, 32));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName),
                new Claim("roles", string.Join(",", roles))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims.ToDictionary(claim => claim.Type, claim => (object)claim.Value),
                Expires = DateTime.Now.ToUniversalTime().AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return await Task.Run(() => tokenHandler.CreateToken(tokenDescriptor));
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            using (var hmac = new System.Security.Cryptography.HMACSHA256(key))
            {
                var randomNumber = new byte[32];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }

                var hash = hmac.ComputeHash(randomNumber);
                return await Task.Run(() => Convert.ToBase64String(hash));
            }
        }

        public async Task<IDictionary<string, string>> ReadClaimsAsync(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var jsonToken = tokenHandler.ReadJsonWebToken(token);
            return await Task.Run(() => jsonToken.Claims.ToDictionary(c => c.Type, c => c.Value));
        }

        public async Task<ICollection<string>> GetRolesFromTokenAsync(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var jsonToken = tokenHandler.ReadJsonWebToken(token);

            var rolesClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "roles")?.Value;
            if (rolesClaim != null)
            {
                return await Task.Run(() => rolesClaim.Split(',').ToList());
            }
            return new List<string>();
        }

        public async Task<string> GetUsernameFromTokenAsync(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var validationResult = await tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            if (!validationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }
            var claimsPrincipal = validationResult.ClaimsIdentity;
            var usernameClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
            return await Task.Run(() => usernameClaim?.Value!);
        }

        public async Task<string> GetUserIdFromTokenAsync(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var validationResult = await tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            if (!validationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }
            var claimsPrincipal = validationResult.ClaimsIdentity;
            var usernameClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            return await Task.Run(() => usernameClaim?.Value!);
        }
        public async Task<DateTime?> GetTokenExpiryAsync(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var validationResult = await tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            if (!validationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }
            var claimsPrincipal = validationResult.ClaimsIdentity;
            var expiryClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
            if (expiryClaim != null)
            {
                var unixTimeSeconds = long.Parse(expiryClaim.Value);
                return await Task.Run(() => DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).DateTime);
            }
            return null;
        }
    }
}
