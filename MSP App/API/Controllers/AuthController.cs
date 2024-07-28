using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.AuthRequests;
using SharedComponents.Entities.WebEntities.Responses.AuthResponses;
using SharedComponents.JWTToken.Services;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Utilities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJWTService _jwtService;
        private readonly IDbRoleService _dbRoleService;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
            IJWTService jwtService, IDbRoleService dbRoleService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtService = jwtService;
            _dbRoleService = dbRoleService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthLoginRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);

                if (result.Succeeded)
                {
                    ApplicationUser? user = await _userManager.FindByNameAsync(request.Username);
                    var roles = await _dbRoleService.GetAllUserRolesByUserIdAsync(user.Id);
                    var token = await _jwtService.GenerateTokenAsync(user.Id, user.UserName, roles.Select(r => r.RoleId).ToList());
                    var refreshToken = await _jwtService.GenerateRefreshTokenAsync();
                    var expiry = await _jwtService.GetTokenExpiryAsync(token);

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = expiry.Value.AddMinutes(_jwtService.JWTSettings.ExpiryMinutes); 
                    await _userManager.UpdateAsync(user);

                    AuthLoginResponse response = new AuthLoginResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        Expires = user.RefreshTokenExpiryTime
                    };
                    return Ok(response);
                }
                return Unauthorized($"Login failed for user '{request.Username}'. Please try again.");
            }
            return BadRequest($"Login failed. Please fill out the username and password and try again.");
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] AuthRefreshRequest request)
        {
            if (ModelState.IsValid)
            {
                var username = await _jwtService.GetUsernameFromTokenAsync(request.Token);
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    return Unauthorized("Invalid token.");
                }

                if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    return Unauthorized("Invalid refresh token.");
                }

                var roles = await _dbRoleService.GetAllUserRolesByUserIdAsync(user.Id);
                var newToken = await _jwtService.GenerateTokenAsync(user.Id, user.UserName, roles.Select(r => r.RoleId).ToList());
                var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync();
                var newExpiry = await _jwtService.GetTokenExpiryAsync(newToken);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = newExpiry.Value.AddMinutes(_jwtService.JWTSettings.ExpiryMinutes);
                await _userManager.UpdateAsync(user);

                var response = new AuthLoginResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Expires = user.RefreshTokenExpiryTime
                };
                return Ok(response);
            }
            return BadRequest("Invalid request. Please provide a valid token and refresh token.");
        }

        //does not invalidate the token after logging out, requires validation tracking and token blacklisting
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if(ModelState.IsValid)
            {
                if (User.Identity != null)
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        return Unauthorized("You are not logged in.");
                    }
                    ApplicationUser? user = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (user != null)
                    {
                        user.RefreshToken = null;
                        user.RefreshTokenExpiryTime = DateTime.MinValue;
                        await _userManager.UpdateAsync(user);
                    }
                    await _signInManager.SignOutAsync();
                    return Ok("Logged out successfully.");
                }
            }
            return BadRequest("Invalid request. Please provide a valid token and refresh token.");
        }
    }
}
