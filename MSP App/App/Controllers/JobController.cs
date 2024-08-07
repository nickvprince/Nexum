using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.JobRequests;
using SharedComponents.Entities.WebEntities.Requests.NASServerRequests;
using SharedComponents.Entities.WebEntities.Requests.TenantRequests;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Utilities;

namespace App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class JobController : Controller
    {
        private readonly IAPIRequestTenantService _tenantService;
        private readonly IAPIRequestDeviceService _deviceService;
        private readonly IAPIRequestNASServerService _nasServerService;
        private readonly IAPIRequestJobService _jobService;

        public JobController(IAPIRequestTenantService tenantService, IAPIRequestDeviceService deviceService,
                       IAPIRequestNASServerService nasServerService, IAPIRequestJobService jobService)
        {
            _tenantService = tenantService;
            _deviceService = deviceService;
            _nasServerService = nasServerService;
            _jobService = jobService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dynamically assign the return URL
            if (HttpContext != null)
            {
                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path);
                HttpContext.Session.SetString("ActiveNavLink", "jobLink");
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
                    if (tenant.Devices != null)
                    {
                        var jobs = await _jobService.GetAllByTenantIdAsync(tenant.Id);
                        if(jobs != null)
                        {
                            foreach (var device in tenant.Devices)
                            {
                                device.Jobs = jobs.Where(j => j.DeviceId == device.Id).ToList();
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
                        t.Devices.Any(d => d.Jobs != null && d.Id == activeDeviceId)).ToList();
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

        [HttpGet("StatusCards")]
        public async Task<IActionResult> StatusCardsAsync()
        {
            return await Task.FromResult(PartialView("_JobStatusCardsPartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpGet("Table")]
        public async Task<IActionResult> TableAsync()
        {
            return await Task.FromResult(PartialView("_JobTablePartial", FilterTenantsBySession(await PopulateTenantsAsync())));
        }

        [HttpGet("Create")]
        public async Task<IActionResult> CreatePartialAsync()
        {
            JobCreateViewModel request = new JobCreateViewModel()
            {
                NASServers = await _nasServerService.GetAllByTenantIdAsync(int.Parse(HttpContext.Session.GetString("ActiveTenantId"))),
                JobCreateRequest = new JobCreateRequest()
                {
                    DeviceId = int.Parse(HttpContext.Session.GetString("ActiveDeviceId")),
                },
            };
            return await Task.FromResult(PartialView("_JobCreatePartial", request));
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAsync(JobCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _deviceService.GetAsync(viewModel.JobCreateRequest.DeviceId);

                if (device != null)
                {
                    if (viewModel.JobCreateRequest.Settings.Type != DeviceJobType.Restore)
                    {
                        ICollection<NASServer>? nasServers = await _nasServerService.GetAllByTenantIdAsync(device.TenantId);
                        if (nasServers != null || nasServers.Any())
                        {
                            if (nasServers.Where(n => n.BackupServerId == viewModel.JobCreateRequest.Settings.BackupServerId).FirstOrDefault() != null)
                            {
                                ICollection<DeviceJob>? jobs = await _jobService.GetAllByDeviceIdAsync(viewModel.JobCreateRequest.DeviceId);
                                if (jobs == null || !jobs.Any())
                                {
                                    DeviceJob? job = await _jobService.CreateAsync(viewModel.JobCreateRequest);
                                    if (job != null)
                                    {
                                        TempData["LastActionMessage"] = "Job created successfully.";
                                        return Json(new { success = true, message = TempData["LastActionMessage"].ToString() });
                                    }
                                    TempData["ErrorMessage"] = "An error occurred while creating the job.";
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Job is already created on this device.";
                                }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "NASServer server not found.";
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No NAS servers found.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "This feature is disabled.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Device not found.";
                }
            }
            viewModel.NASServers = await _nasServerService.GetAllByTenantIdAsync(int.Parse(HttpContext.Session.GetString("ActiveTenantId")));
            string html = await RenderUtilities.RenderViewToStringAsync(this, "Job/_JobCreatePartial", viewModel);
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString(), html });
        }

        [HttpGet("{id}/Update")]
        public async Task<IActionResult> UpdatePartialAsync(int id)
        {
            JobUpdateViewModel viewModel = new JobUpdateViewModel();
            
            DeviceJob? job = await _jobService.GetAsync(id);
            if (job != null)
            {
                Device? device = await _deviceService.GetAsync((int)job.DeviceId);
                if (device != null)
                {
                    viewModel.NASServers = await _nasServerService.GetAllByTenantIdAsync(device.TenantId);
                }
                viewModel.JobUpdateRequest = new JobUpdateRequest();
                viewModel.JobUpdateRequest.Id = job.Id;
                viewModel.JobUpdateRequest.Name = job.Name;
                viewModel.JobUpdateRequest.Settings = new JobInfoRequest();
                if(job.Settings != null)
                {
                    viewModel.JobUpdateRequest.Settings.BackupServerId = (int)job.Settings.BackupServerId;
                    viewModel.JobUpdateRequest.Settings.Type = job.Settings.Type;
                    viewModel.JobUpdateRequest.Settings.Retention = job.Settings.Retention;
                    viewModel.JobUpdateRequest.Settings.RetryCount = job.Settings.RetryCount;
                    viewModel.JobUpdateRequest.Settings.Sampling = job.Settings.Sampling;
                    viewModel.JobUpdateRequest.Settings.StartTime = job.Settings.StartTime;
                    viewModel.JobUpdateRequest.Settings.EndTime = job.Settings.EndTime;
                    viewModel.JobUpdateRequest.Settings.UpdateInterval = job.Settings.UpdateInterval;
                    viewModel.JobUpdateRequest.Settings.Schedule = new JobScheduleRequest();
                    if(job.Settings.Schedule != null)
                    {
                        viewModel.JobUpdateRequest.Settings.Schedule.Sunday = job.Settings.Schedule!.Sunday;
                        viewModel.JobUpdateRequest.Settings.Schedule.Monday = job.Settings.Schedule.Monday;
                        viewModel.JobUpdateRequest.Settings.Schedule.Tuesday = job.Settings.Schedule.Tuesday;
                        viewModel.JobUpdateRequest.Settings.Schedule.Wednesday = job.Settings.Schedule.Wednesday;
                        viewModel.JobUpdateRequest.Settings.Schedule.Thursday = job.Settings.Schedule.Thursday;
                        viewModel.JobUpdateRequest.Settings.Schedule.Friday = job.Settings.Schedule.Friday;
                        viewModel.JobUpdateRequest.Settings.Schedule.Saturday = job.Settings.Schedule.Saturday;
                    }
                }
            }
            return await Task.FromResult(PartialView("_JobUpdatePartial", viewModel));
        }

        [HttpPost("{id}/Update")]
        public async Task<IActionResult> UpdateTenantAsync(JobUpdateViewModel viewModel)
        {
            DeviceJob? job = await _jobService.GetAsync(viewModel.JobUpdateRequest.Id);
            if (job != null)
            {
                Device? device = await _deviceService.GetAsync((int)job.DeviceId);
                if(device != null)
                {
                    viewModel.NASServers = await _nasServerService.GetAllByTenantIdAsync(device.TenantId);
                }
                if (ModelState.IsValid)
                {
                    job = await _jobService.UpdateAsync(viewModel.JobUpdateRequest);
                    if (job != null)
                    {
                        TempData["LastActionMessage"] = "Job updated successfully.";
                        return Json(new { success = true, message = TempData["LastActionMessage"].ToString() });
                    }
                }
            }
            TempData["ErrorMessage"] = "An error occurred while updating the job.";
            string html = await RenderUtilities.RenderViewToStringAsync(this, "Job/_JobUpdatePartial", viewModel);
            return Json(new { success = false, message = TempData["ErrorMessage"].ToString(), html });
        }

        [HttpPost("{id}/Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _jobService.DeleteAsync(id))
                {
                    TempData["LastActionMessage"] = "Job deleted successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the job.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }

        [HttpPost("{id}/Start")]
        public async Task<IActionResult> StartAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _jobService.StartAsync(id))
                {
                    TempData["LastActionMessage"] = "Job started successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while starting the job.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }

        [HttpPost("{id}/Stop")]
        public async Task<IActionResult> StopAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _jobService.StopAsync(id))
                {
                    TempData["LastActionMessage"] = "Job stopped successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while stopping the job.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }

        [HttpPost("{id}/Resume")]
        public async Task<IActionResult> ResumeAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _jobService.ResumeAsync(id))
                {
                    TempData["LastActionMessage"] = "Job resumed successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while resuming the job.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }

        [HttpPost("{id}/Pause")]
        public async Task<IActionResult> PauseAsync(int id)
        {
            if (ModelState.IsValid)
            {
                if (await _jobService.PauseAsync(id))
                {
                    TempData["LastActionMessage"] = "Job paused successfully.";
                    return Json(new { success = true, message = TempData["LastActionMessage"]?.ToString() });
                }
                TempData["ErrorMessage"] = "An error occurred while pausing the job.";
            }
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() });
        }
    }
}
