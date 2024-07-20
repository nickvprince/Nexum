using App.Models;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    public class TenantController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestAlertService _alertService;
        private readonly IAPIRequestLogService _logService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestRoleService _roleService;
        private readonly IAPIRequestPermissionService _permissionService;
        private readonly IAPIRequestInstallationKeyService _installationKeyService;

        public TenantController(IAPIRequestTenantService tenantService, IAPIRequestAlertService alertService,
            IAPIRequestLogService logService, IAPIRequestInstallationKeyService installationKeyService,
            IAPIRequestPermissionService permissionService, IAPIRequestRoleService roleService,
            IAPIRequestDeviceService deviceService)
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
