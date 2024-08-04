using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Net;

namespace App.Middleware
{
    public class AppAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (!authorizeResult.Succeeded)
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Auth/Index?ReturnUrl={WebUtility.UrlEncode(returnUrl)}");
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
