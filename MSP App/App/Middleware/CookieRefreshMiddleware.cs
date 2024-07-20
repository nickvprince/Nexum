using Microsoft.AspNetCore.Authentication;
using SharedComponents.Entities.WebEntities.Requests.AuthRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Utilities;
using System.Security.Claims;

namespace App.Middleware
{
    public class CookieRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, IAPIRequestAuthService authService)
        {
            var accessToken = context.User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            var refreshToken = context.User.Claims.FirstOrDefault(c => c.Type == "RefreshToken")?.Value;
            var expires = context.User.Claims.FirstOrDefault(c => c.Type == "Expires")?.Value;

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken) && DateTime.TryParse(expires, out var expiryDate))
            {
                if (expiryDate <= DateTimeUtilities.EstNow().AddMinutes(5)) // If the token expires in less than 5 minutes
                {
                    var refreshRequest = new AuthRefreshRequest
                    {
                        Token = accessToken,
                        RefreshToken = refreshToken
                    };

                    var refreshResponse = await authService.RefreshAsync(refreshRequest);
                    if (refreshResponse != null)
                    {
                        // Update the cookie claims with the new token details
                        var claims = context.User.Claims.ToList();
                        claims.RemoveAll(c => c.Type == "Token" || c.Type == "RefreshToken" || c.Type == "Expires");

                        claims.Add(new Claim("Token", refreshResponse.Token));
                        claims.Add(new Claim("RefreshToken", refreshResponse.RefreshToken));
                        claims.Add(new Claim("Expires", refreshResponse.Expires.ToString()));

                        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await context.SignInAsync("Cookies", claimsPrincipal, new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = refreshResponse.Expires
                        });
                    }
                }
            }
            await _next(context);
        }
    }
}
