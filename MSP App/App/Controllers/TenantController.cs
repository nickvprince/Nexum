using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class TenantController : Controller
    {
        private readonly TenantService _tenantService;

        public TenantController(TenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            TenantViewModel tenantViewModel = new TenantViewModel
            {
                Tenants = await _tenantService.GetAllAsync()
            };

            return View(tenantViewModel);
        }
    }
}
