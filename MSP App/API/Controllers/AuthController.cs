using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest  loginRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginRequest.Username, loginRequest.Password, false, false);

                if (result.Succeeded)
                {
                    ApplicationUser? user = await _userManager.FindByNameAsync(loginRequest.Username);

                    var response = new
                    {
                        data = user,
                        message = $"'{loginRequest.Username}' - Login Successful."
                    };
                    return Ok(response);
                }
                return Unauthorized(new { Message = $"Login failed for user '{loginRequest.Username}'. Please try again." });
            }
            return BadRequest(new { Message = $"Login failed. Please fill out the username and password and try again." });
        }
    }
}
