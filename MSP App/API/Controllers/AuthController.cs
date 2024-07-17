﻿using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;
using SharedComponents.JWTToken.Services;
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

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                    await _userManager.UpdateAsync(user);

                    AuthLoginResponse response = new AuthLoginResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        Expires = DateTime.UtcNow.AddMinutes(_jwtService.JWTSettings.ExpiryMinutes)
                    };
                    return Ok(response);
                }
                return Unauthorized($"Login failed for user '{request.Username}'. Please try again.");
            }
            return BadRequest($"Login failed. Please fill out the username and password and try again.");
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] AuthTokenRefreshRequest request)
        {
            if (ModelState.IsValid)
            {
                var username = await _jwtService.GetUsernameFromExpiredToken(request.Token);
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    return Unauthorized("Invalid token.");
                }
                if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Unauthorized("Invalid refresh token.");
                }

                var roles = await _dbRoleService.GetAllUserRolesByUserIdAsync(user.Id);
                var newToken = await _jwtService.GenerateTokenAsync(user.Id, user.UserName, roles.Select(r => r.RoleId).ToList());
                var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                var response = new AuthLoginResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Expires = DateTime.UtcNow.AddMinutes(_jwtService.JWTSettings.ExpiryMinutes)
                };
                return Ok(response);
            }
            return BadRequest("Invalid request. Please provide a valid token and refresh token.");
        }
    }
}
