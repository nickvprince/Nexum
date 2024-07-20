using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SharedComponents.Handlers.Results
{
    public class CustomForbidResult : ForbidResult
    {
        public string Message { get; }

        public CustomForbidResult(string message) : base(new AuthenticationProperties())
        {
            Message = message;
        }
        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.HttpContext.Response.WriteAsync(Message);
        }
    }
}
