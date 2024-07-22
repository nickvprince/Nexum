using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.AuthRequests;
using SharedComponents.WebEntities.Responses.AuthResponses;
using SharedComponents.WebRequestEntities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
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
        public async Task<IActionResult> LoginAsync([FromBody] AuthLoginRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);

                if (result.Succeeded)
                {
                    //ApplicationUser? user = await _userManager.FindByNameAsync(request.Username);

                    AuthLoginResponse response = new AuthLoginResponse
                    {
                        /*Token = TokenUtilities.GenerateToken(user),
                        RefreshToken = TokenUtilities.GenerateRefreshToken(user),
                        Expires = TokenUtilities.GetTokenExpiration()*/
                        Token = request.Username + " - Token",
                        RefreshToken = request.Username + " - RefreshToken",
                        Expires = DateTime.Now.AddDays(1)
                    };
                    return Ok(response);
                }
                return Unauthorized($"Login failed for user '{request.Username}'. Please try again.");
            }
            return BadRequest($"Login failed. Please fill out the username and password and try again.");
        }
    }
}
