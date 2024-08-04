using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
                if (expiryDate <= DateTime.Now.ToUniversalTime().AddMinutes(5)) // If the token expires in less than 5 minutes
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
                        claims.RemoveAll(c => c.Type == "Token" || c.Type == "RefreshToken" || c.Type == "Expires" || c.Type == "ExpiresEST");

                        claims.Add(new Claim("Token", refreshResponse.Token));
                        claims.Add(new Claim("RefreshToken", refreshResponse.RefreshToken));
                        claims.Add(new Claim("Expires", refreshResponse.Expires.ToString()));
                        claims.Add(new Claim("ExpiresEST", DateTimeUtilities.ConvertToEst((DateTime)refreshResponse.Expires).ToString()));

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = refreshResponse.Expires
                        });
                    }
                    else
                    {
                        if (context.User.Identity.IsAuthenticated)
                        {
                            await context.SignOutAsync("Cookies");
                            var returnUrl = context.Request.Path + context.Request.QueryString;
                            if (!returnUrl.Contains("Auth"))
                            {
                                var loginUrl = $"/Auth/Index?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
                                context.Response.Redirect(loginUrl);
                            }
                            else
                            {
                                context.Response.Redirect("/Auth/Index");
                            }
                            return;
                        }
                    }
                }
            }
            await _next(context);
        }
    }
}
