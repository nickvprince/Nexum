using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;
using System.Diagnostics;

namespace App.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestNASServerService _nasServerService;
        private readonly IAPIRequestJobService _jobService;
        private readonly IAPIRequestAlertService _alertService;
        private readonly IAPIRequestLogService _logService;
        private readonly IAPIRequestBackupService _backupService;

        public HomeController(ILogger<HomeController> logger, IAPIRequestTenantService tenantService,
            IAPIRequestDeviceService deviceService, IAPIRequestNASServerService nasServerService,
            IAPIRequestJobService jobService, IAPIRequestAlertService alertService,
            IAPIRequestLogService logService, IAPIRequestBackupService backupService)
        {
            _logger = logger;
            _tenantService = tenantService;
            _deviceService = deviceService;
            _nasServerService = nasServerService;
            _jobService = jobService;
            _alertService = alertService;
            _logService = logService;
            _backupService = backupService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "homeLink");
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
                    if (tenant.Devices != null)
                    {
                        var jobs = await _jobService.GetAllByTenantIdAsync(tenant.Id);
                        if (jobs != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Jobs = jobs.Where(j => j.DeviceId == device.Id).ToList();
                            }
                        }
                        var logs = await _logService.GetAllByTenantIdAsync(tenant.Id);
                        if (logs != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Logs = logs.Where(l => l.DeviceId == device.Id).ToList();
                            }
                        }
                        var alerts = await _alertService.GetAllByTenantIdAsync(tenant.Id);
                        if (alerts != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Alerts = alerts.Where(a => a.DeviceId == device.Id).ToList();
                            }
                        }
                    }
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
                return tenants?.Where(t => t.Devices != null &&
                        t.Devices.Any(d => d.Jobs != null)).ToList();
            }
            if (HttpContext.Session.GetString("ActiveTenantId") != null)
            {
                int? activeTenantId = int.Parse(HttpContext.Session.GetString("ActiveTenantId"));
                return tenants?.Where(t => t.Id == activeTenantId).ToList();
            }
            return tenants;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return await Task.FromResult(View());
        }

        [HttpGet("Privacy")]
        public async Task<IActionResult> PrivacyAsync()
        {
            return await Task.FromResult(View());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet("SummaryCards")]
        public async Task<IActionResult> SummaryCardsAsync()
        {
            return await Task.FromResult(PartialView("_HomeSummaryCardsPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [Authorize]
        [HttpGet("AlertSummaryCard")]
        public async Task<IActionResult> AlertSummaryCardAsync()
        {
            return await Task.FromResult(PartialView("_HomeAlertSummaryCardPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [Authorize]
        [HttpGet("LogSummaryCard")]
        public async Task<IActionResult> LogSummaryCardAsync()
        {
            return await Task.FromResult(PartialView("_HomeLogSummaryCardPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [Authorize]
        [HttpGet("JobSummaryCard")]
        public async Task<IActionResult> JobSummaryCardAsync()
        {
            return await Task.FromResult(PartialView("_HomeJobSummaryCardPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [Authorize]
        [HttpGet("BackupSummaryCard")]
        public async Task<IActionResult> BackupSummaryCardAsync()
        {
            return await Task.FromResult(PartialView("_HomeBackupSummaryCardPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }
        
    }
}
