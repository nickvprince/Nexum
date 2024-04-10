using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginViewModel loginModel)
        {
            // Authentication logic here...
            // For demo purposes, let's just assume successful login
            return Ok($"User {loginModel.Username} - {loginModel.Password} logged in successfully.");
        }
    }
}
