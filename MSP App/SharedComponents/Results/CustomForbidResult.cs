using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Results
{
    public class CustomForbidResult : ForbidResult
    {
        public string Message { get; }

        public CustomForbidResult(string message): base(new AuthenticationProperties())
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
