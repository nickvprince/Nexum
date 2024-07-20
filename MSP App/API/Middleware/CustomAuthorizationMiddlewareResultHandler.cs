using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Net;

namespace API.Middleware
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Challenged)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                var failureReasons = authorizeResult.AuthorizationFailure?.FailureReasons.Select(f => f.Message);
                var message = failureReasons != null && failureReasons.Any() ? string.Join(", ", failureReasons) : "Unauthorized access";

                await context.Response.WriteAsync($"{message}");
                return;
            }
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                var failureReasons = authorizeResult.AuthorizationFailure?.FailureReasons.Select(f => f.Message);
                var message = failureReasons != null && failureReasons.Any() ? string.Join(", ", failureReasons) : "Forbidden access";

                await context.Response.WriteAsync($"{message}");
                return;
            }
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
