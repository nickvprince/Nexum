using App.Models;
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
    public class RoleController : Controller
    {
        private readonly IAPIRequestRoleService _roleService;
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestPermissionService _permissionService;

        public RoleController(IAPIRequestRoleService roleService, IAPIRequestTenantService tenantService,
            IAPIRequestPermissionService _permissionService)
        {
            _roleService = roleService;
            _tenantService = tenantService;
            this._permissionService = _permissionService;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "roleLink");
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
            RoleViewModel roleViewModel = new RoleViewModel();
            roleViewModel.Roles = await _roleService.GetAllAsync();
            if (roleViewModel.Roles != null)
            {
                var permissions = await _permissionService.GetAllAsync();
                foreach (var role in roleViewModel.Roles)
                {
                    role.RolePermissions = await _roleService.GetAllRolePermissionByIdAsync(role.Id);
                    if (role.RolePermissions != null)
                    {
                        foreach (var rolePermission in role.RolePermissions)
                        {
                            rolePermission.Permission = permissions.FirstOrDefault(p => p.Id == rolePermission.PermissionId);
                        }
                    }
                }
            }
            roleViewModel.Tenants = await _tenantService.GetAllAsync();
            return await Task.FromResult(PartialView("_RoleTablePartial", roleViewModel));
        }
    }
}
