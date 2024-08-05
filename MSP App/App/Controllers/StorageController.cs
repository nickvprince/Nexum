using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.InstallationKeyRequests;
using SharedComponents.Entities.WebEntities.Requests.NASServerRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Utilities;

namespace App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class StorageController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestNASServerService _nasServerService;
        private readonly IAPIRequestBackupService _backupService;
        private readonly IAPIRequestJobService _jobService;

        public StorageController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService,
            IAPIRequestNASServerService nasServerService, IAPIRequestBackupService backupService,
            IAPIRequestJobService _jobService)
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

        private async Task<ICollection<Tenant>?> PopulateTenantsAsync()
        {
            var tenants = await _tenantService.GetAllAsync();
            if (tenants != null)
            {
                foreach (var tenant in tenants)
                {
                    tenant.Devices = await _deviceService.GetAllByTenantIdAsync(tenant.Id);
                    tenant.NASServers = await _nasServerService.GetAllByTenantIdAsync(tenant.Id);
                    if (tenant.NASServers != null)
                    {
                        var backups = await _backupService.GetAllByTenantIdAsync(tenant.Id);
                        if (backups != null)
                        {
                            foreach (var nasServer in tenant.NASServers)
                            {
                                nasServer.Backups = backups.Where(b => b.NASServerId == nasServer.Id).ToList();
                            }
                        }
                    }
                }
            }
            return tenants;
        }

        private ICollection<Tenant>? FilterTenantsBySession(ICollection<Tenant>? tenants)
        {
            if (HttpContext.Session.GetString("ActiveDeviceId") != null)
            {
                int? activeDeviceId = int.Parse(HttpContext.Session.GetString("ActiveDeviceId"));
                return tenants?.Where(t =>
                    t.NASServers != null &&
                    t.Devices != null &&
                    t.NASServers.Any(n =>
                        (n.Backups != null && n.Backups.Any(b =>
                            b.Client_Id == t.Devices
                                .Where(dn => dn.Id == activeDeviceId)
                                .Select(dn => dn.DeviceInfo?.ClientId)
                                .FirstOrDefault()
                        ))
                        ||
                        (t.Devices.Any(dj =>
                            dj.Jobs != null &&
                            dj.Jobs.Any(j =>
                                j.Settings != null &&
                                j.Settings.BackupServerId == n.BackupServerId &&
                                j.DeviceId == activeDeviceId
                            )
                        ))
                    )
                ).ToList();
            }
            if (HttpContext.Session.GetString("ActiveTenantId") != null)
            {
                int? activeTenantId = int.Parse(HttpContext.Session.GetString("ActiveTenantId"));
                return tenants?.Where(t => t.Id == activeTenantId).ToList();
            }
            return tenants;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet("NASTable")]
        public async Task<IActionResult> NASTableAsync()
        {
            return await Task.FromResult(PartialView("_NASTablePartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpGet("BackupTable")]
        public async Task<IActionResult> BackupTableAsync()
        {
            return await Task.FromResult(PartialView("_BackupTablePartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpGet("{id}/CreateNAS")]
        public async Task<IActionResult> CreateNASPartialAsync(int id)
        {
            NASServerCreateRequest request = new NASServerCreateRequest
            {
                TenantId = id
            };
            return await Task.FromResult(PartialView("_NASCreatePartial", request));
        }

        [HttpPost("{id}/CreateNAS")]
        public async Task<IActionResult> CreateNASAsync(NASServerCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _nasServerService.CreateAsync(request);
                if (nasServer != null)
                {
                    TempData["LastActionMessage"] = "NAS server created successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"].ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while creating the NAS server.";
            }
            string html = await RenderUtilities.RenderViewToStringAsync(this, "Storage/_NASCreatePartial", request);
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString(), html });
        }

        [HttpGet("{id}/UpdateNAS")]
        public async Task<IActionResult> UpdateNASPartialAsync(int id)
        {
            NASServer? nasServer = await _nasServerService.GetAsync(id);
            if (nasServer != null)
            {
                NASServerUpdateRequest request = new NASServerUpdateRequest
                {
                    Id = nasServer.Id,
                    Name = nasServer.Name,
                    Path = nasServer.Path
                };
                return await Task.FromResult(PartialView("_NASUpdatePartial", request));
            }
            TempData["ErrorMessage"] = "An error occurred while retrieving the NAS server information.";
            return await Task.FromResult(PartialView("_NASUpdatePartial"));
        }

        [HttpPost("{id}/UpdateNAS")]
        public async Task<IActionResult> UpdateNASAsync(NASServerUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                NASServer? nasServer = await _nasServerService.UpdateAsync(request);
                if (nasServer != null)
                {
                    TempData["LastActionMessage"] = "NAS server updated successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"].ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while updating the NAS server.";
            }
            string html = await RenderUtilities.RenderViewToStringAsync(this, "Storage/_NASUpdatePartial", request);
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString(), html });
        }

        [HttpPost("{id}/DeleteNAS")]
        public async Task<IActionResult> DeleteNASAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _nasServerService.DeleteAsync(id))
                {
                    TempData["LastActionMessage"] = "NAS server deleted successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the NAS server.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }

        [HttpPost("{id}/DeleteBackup")]
        public async Task<IActionResult> DeleteBackupAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _backupService.DeleteAsync(id))
                {
                    TempData["LastActionMessage"] = "Backup deleted successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the backup.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }
        
    }
}
