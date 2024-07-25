using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.WebAppEntities.Requests.Session_Requests;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        public SessionController() { }

        [HttpPost("")]
        public async Task<IActionResult> SetActiveNavLink([FromBody] SetActiveNavLinkRequest request)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(request.ActiveLinkId))
                {
                    HttpContext.Session.SetString("ActiveNavLink", request.ActiveLinkId);
                    return await Task.FromResult(Ok());
                }
            }
            return await Task.FromResult(BadRequest("Invalid active link ID."));
        }
    }
}
