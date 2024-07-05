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

        public TenantController(TenantService tenantService, AlertService alertService, LogService logService)
        {
            _tenantService = tenantService;
            _alertService = alertService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            TenantViewModel tenantViewModel = new TenantViewModel
            {
                Tenants = await _tenantService.GetAllAsync()
            };

            // Example usage of the alert service

            DeviceAlert? res1 = await _alertService.CreateAsync(new DeviceAlert 
            { 
                Message = "Alert created.", 
                Severity = AlertSeverity.Medium, 
                Time = DateTime.Now, 
                DeviceId = 1
            });

            DeviceAlert? res2 = await _alertService.EditAsync(new DeviceAlert
            {
                Id = 1,
                Message = "Alert edited.",
                Severity = AlertSeverity.Medium,
                Time = DateTime.Now,
                DeviceId = 1
            });

            bool resBool = await _alertService.DeleteAsync(1);

            DeviceAlert? res3 = await _alertService.GetAsync(1);

            ICollection<DeviceAlert>? alerts = await _alertService.GetAllAsync();

            // Example usage of the device log service

            DeviceLog? dev1 = await _logService.CreateAsync(new DeviceLog
            {
                Message = "Log created.",
                Time = DateTime.Now,
                DeviceId = 1
            });

            bool resBool2 = await _logService.DeleteAsync(1);

            DeviceLog? dev3 = await _logService.GetAsync(1);

            ICollection<DeviceLog>? logs = await _logService.GetAllAsync();

            // Example usage of the tenant service

            Tenant? newTenant = await _tenantService.CreateAsync(new Tenant
            {
                Name = "New Test Tenant",
                TenantInfo = new TenantInfo
                {
                    Name = "Test Person",
                    Address = "123 Main",
                    City = "Test City",
                    State = "TS",
                    Zip = "12345",
                    Phone = "123-456-7890",
                    Email = "123@123.com"
                }
            });

            Tenant? editedTenant = await _tenantService.EditAsync(new Tenant
            {
                Id = 1,
                Name = "Edited Test Tenant",
                TenantInfo = new TenantInfo
                {
                    Id = 1,
                    Name = "Edited Test Person",
                    Address = "123 Main",
                    City = "Test City",
                    State = "TS",
                    Zip = "12345",
                    Phone = "123-456-7890",
                    Email = "123@123.com"
                }
            });

            bool resBool3 = await _tenantService.DeleteAsync(2);

            Tenant? tenant = await _tenantService.GetAsync(1);

            ICollection<Tenant>? tenants = await _tenantService.GetAllAsync();

            return View(tenantViewModel);
        }
    }
}
