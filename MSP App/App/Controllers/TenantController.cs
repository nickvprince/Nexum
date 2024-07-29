using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Authorize]
    public class TenantController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestInstallationKeyService _installationKeyService;

        public TenantController(IAPIRequestTenantService tenantService, IAPIRequestInstallationKeyService installationKeyService)
        {
            _tenantService = tenantService;
            _installationKeyService = installationKeyService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "tenantLink");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet]
        public async Task<IActionResult> TableAsync()
        {
            ICollection<Tenant>? tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.InstallationKeys = await _installationKeyService.GetAllByTenantIdAsync(tenant.Id);
                }
            }
            return await Task.FromResult(PartialView("_TenantTablePartial", tenants));
        }
    }
}
