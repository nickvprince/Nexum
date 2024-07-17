using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.JWTToken.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.JWTToken.Services
{
    public class JWTService : IJWTService
    {
        private readonly JWTSettings? _jwtSettings;

        public JWTService(IConfiguration config)
        {
            _jwtSettings = config.GetSection("JWTSettings").Get<JWTSettings>();
        }

        public async Task<string> PadKey(string key, int length)
        {
            if (key.Length >= length)
            {
                return await Task.Run(() => key.Substring(0, length));
            }
            return await Task.Run(() => key.PadRight(length, '0'));
        }

        public JWTSettings JWTSettings => _jwtSettings ?? throw new NullReferenceException("JWTSettings is null");

        public async Task<string> GenerateTokenAsync(string userId, string userName, List<string> roles)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var key = Encoding.ASCII.GetBytes(await PadKey(_jwtSettings.SecretKey, 32));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>()
            };
            foreach (var claim in claims)
            {
                tokenDescriptor.Claims[claim.Type] = claim.Value;
            }
            tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
            tokenDescriptor.Issuer = _jwtSettings.Issuer;
            tokenDescriptor.Audience = _jwtSettings.Audience;
            tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

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

        public async Task<string> GetUsernameFromExpiredToken(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var key = Encoding.ASCII.GetBytes(await PadKey(_jwtSettings.SecretKey, 32));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            var validationResult = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
            if (!validationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }
            var claimsPrincipal = validationResult.ClaimsIdentity;
            var usernameClaim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
            return await Task.Run(() => usernameClaim?.Value!);
        }
    }
}
