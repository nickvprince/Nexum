﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.RequestEntities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager)
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
                    var appUser = await _userManager.FindByNameAsync(loginRequest.Username);

                    var response = new
                    {
                        appUser,
                        Message = $"'{loginRequest.Username}' - Login Successful."
                    };
                    return Ok(response);
                }
                return Unauthorized(new { Message = $"Login failed for user '{loginRequest.Username}'. Please try again." });
            }
            return BadRequest(new { Message = $"Login failed. Please fill out the username and password and try again." });
        }
    }
}
