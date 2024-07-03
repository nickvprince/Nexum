using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace App.Controllers
{
    public class TenantController : Controller
    {
        private readonly TenantService _tenantService;
        private readonly AlertService _alertService;
        private readonly LogService _logService;
        private readonly DeviceService _deviceService;
        private readonly RoleService _roleService;
        private readonly PermissionService _permissionService;
        private readonly InstallationKeyService _installationKeyService;

        public TenantController(TenantService tenantService, AlertService alertService, 
            LogService logService, InstallationKeyService installationKeyService, 
            PermissionService permissionService, RoleService roleService,
            DeviceService deviceService)
        {
            _tenantService = tenantService;
            _alertService = alertService;
            _logService = logService;
            _installationKeyService = installationKeyService;
            _permissionService = permissionService;
            _roleService = roleService;
            _deviceService = deviceService;
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
