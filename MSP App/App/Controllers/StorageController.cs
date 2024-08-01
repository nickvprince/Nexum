using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class StorageController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestNASServerService _nasServerService;
        private readonly IAPIRequestBackupService _backupService;

        public StorageController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService,
            IAPIRequestNASServerService nasServerService, IAPIRequestBackupService backupService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
            _nasServerService = nasServerService;
            _backupService = backupService;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "storageLink");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            ICollection<Tenant>? tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.Devices = await _deviceService.GetAllByTenantIdAsync(tenant.Id);
                    tenant.NASServers = await _nasServerService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.NASServers != null)
                    {
                        ICollection<DeviceBackup>? backups = await _backupService.GetAllByTenantIdAsync(tenant.Id);
                        foreach (var nasServer in tenant.NASServers)
                        {
                            nasServer.Backups = backups.Where(b => b.NASServerId == nasServer.Id).ToList();
                        }
                    }
                }
            }
            return await Task.FromResult(View(tenants));
        }

        [HttpGet("NASTable")]
        public async Task<IActionResult> NASTableAsync()
        {
            ICollection<Tenant>? tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.Devices = await _deviceService.GetAllByTenantIdAsync(tenant.Id);
                    tenant.NASServers = await _nasServerService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.NASServers != null)
                    {
                        ICollection<DeviceBackup>? backups = await _backupService.GetAllByTenantIdAsync(tenant.Id);
                        foreach (var nasServer in tenant.NASServers)
                        {
                            nasServer.Backups = backups.Where(b => b.NASServerId == nasServer.Id).ToList();
                        }
                    }
                }
            }
            if (HttpContext.Session.GetString("ActiveTenantId") != null)
            {
                int? ActiveTenantId = int.Parse(HttpContext.Session.GetString("ActiveTenantId"));
                return await Task.FromResult(PartialView("_NASTablePartial", tenants.Where(t => t.Id == ActiveTenantId).ToList()));
            }
            return await Task.FromResult(PartialView("_NASTablePartial", tenants));
        }

        [HttpGet("BackupTable")]
        public async Task<IActionResult> BackupTableAsync()
        {
            ICollection<Tenant>? tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.NASServers = await _nasServerService.GetAllByTenantIdAsync(tenant.Id);
                }
                foreach (var tenant in tenants)
                {
                    if (tenant.NASServers != null) {
                        foreach (var nasServer in tenant.NASServers)
                        {
                            nasServer.Backups = (await _backupService.GetAllByTenantIdAsync(tenant.Id)).Where(b => b.NASServerId == nasServer.Id).ToList();
                        }
                    }
                }
            }
            return await Task.FromResult(PartialView("_BackupTablePartial", tenants));
        }
    }
}
