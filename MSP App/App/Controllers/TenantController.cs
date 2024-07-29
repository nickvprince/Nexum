using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.InstallationKeyRequests;
using SharedComponents.Entities.WebEntities.Requests.TenantRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Route("[controller]")]
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

        [HttpGet("Table")]
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

        [HttpGet("Create")]
        public async Task<IActionResult> CreateTenantPartialAsync()
        {
            return await Task.FromResult(PartialView("_TenantCreatePartial"));
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateTenantAsync(TenantCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Tenant? tenant = await _tenantService.CreateAsync(request);
                if (tenant != null)
                {
                    return RedirectToAction("Index", "Tenant");
                }
            }
            return await Task.FromResult(PartialView("_TenantCreatePartial", request));
        }

        [HttpGet("{id}/CreateKey")]
        public async Task<IActionResult> CreateKeyPartialAsync(int id)
        {
            InstallationKeyCreateRequest request = new InstallationKeyCreateRequest
            {
                TenantId = id
            };
            return await Task.FromResult(PartialView("_KeyCreatePartial", request));
        }
    }
}
