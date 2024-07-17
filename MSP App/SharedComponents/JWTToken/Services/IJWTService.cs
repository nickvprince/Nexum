using Microsoft.AspNetCore.Identity;
using SharedComponents.Entities;
using SharedComponents.JWTToken.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.JWTToken.Services
{
    public interface IJWTService
    {
        public JWTSettings JWTSettings { get; }
        public Task<string> PadKey (string key, int length);
        public Task<string> GenerateTokenAsync(string userId, string userName, List<string> roles);
        public Task<string> GenerateRefreshTokenAsync();
        public Task<IDictionary<string, string>> ReadClaimsAsync(string token);
        public Task<string> GetUsernameFromExpiredToken(string token);
    }
}
