using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SharedComponents.Utilities
{
    public class AuthCookieUtilities
    {
        public static IDictionary<string, string> GetClaims(HttpContext context)
        {
            var claims = new Dictionary<string, string>();

            if (context.User.Identity.IsAuthenticated)
            {
                foreach (var claim in context.User.Claims)
                {
                    claims[claim.Type] = claim.Value;
                }
            }
            return claims;
        }
    }
}
