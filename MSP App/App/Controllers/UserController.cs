using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Responses.UserResponses;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IAPIRequestUserService _userService;
        private readonly IAPIRequestRoleService _roleService;

        public UserController(IAPIRequestUserService userService, IAPIRequestRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "userLink");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet("Table")]
        public async Task<IActionResult> TableAsync()
        {
            ICollection<UserResponse>? userResponses = await _userService.GetAllAsync();
            ICollection<ApplicationUser>? users = new List<ApplicationUser>();
            if (userResponses != null)
            {
                foreach (var userResponse in userResponses)
                {
                    users.Add(new ApplicationUser
                    {
                        Id = userResponse.Id,
                        UserName = userResponse.UserName,
                        FirstName = userResponse.FirstName,
                        LastName = userResponse.LastName,
                        Type = userResponse.Type
                    });
                }
            }
            foreach (var user in users)
            {
                user.UserRoles = await _roleService.GetAllUserRolesByUserIdAsync(user.Id);
                if (user.UserRoles != null)
                {
                    var roles = await _roleService.GetAllAsync();
                    if(roles != null)
                    {
                        foreach (var userRole in user.UserRoles)
                        {
                            userRole.Role = roles.Where(r => r.Id == userRole.RoleId).FirstOrDefault();
                        }
                    }
                }
            }
            return await Task.FromResult(PartialView("_UserTablePartial", users));
        }
    }
}
