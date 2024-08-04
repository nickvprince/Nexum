using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.JobRequests;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.Handlers.Results;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Services.TenantServerAPIServices.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class JobController : ControllerBase
    {
        private readonly IDbJobService _dbJobService;
        private readonly ITenantServerAPIJobService _httpJobService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IDbNASServerService _dbNASServerService;
        private readonly IAPIAuthService _authService;

        public JobController(IDbJobService dbJobService, ITenantServerAPIJobService httpJobService, 
            IDbDeviceService dbDeviceService, IDbNASServerService dbNASServerService,
            IAPIAuthService authService)
        {
            _dbJobService = dbJobService;
            _httpJobService = httpJobService;
            _dbDeviceService = dbDeviceService;
            _dbNASServerService = dbNASServerService;
            _authService = authService;
        }

        [HttpPost("")]
        [HasPermission("Job.Create.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> CreateAsync([FromBody] JobCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (request.Settings != null)
                {
                    NASServer? nasServer = await _dbNASServerService.GetByBackupServerIdAsync(device.TenantId, request.Settings.BackupServerId);
                    if (nasServer == null)
                    {
                        return NotFound("NAS Server not found.");
                    }
                    if (request.Settings.Schedule != null)
                    {
                        DeviceJob? job = new DeviceJob
                        {
                            DeviceId = request.DeviceId,
                            Name = request.Name,
                            Settings = new DeviceJobInfo
                            {
                                BackupServerId = request.Settings.BackupServerId,
                                Type = request.Settings.Type,
                                StartTime = request.Settings.StartTime,
                                EndTime = request.Settings.EndTime,
                                UpdateInterval = request.Settings.UpdateInterval,
                                retryCount = request.Settings.retryCount,
                                Sampling = request.Settings.Sampling,
                                Retention = request.Settings.Retention,

                                Schedule = new DeviceJobSchedule
                                {
                                    Sunday = request.Settings.Schedule.Sunday,
                                    Monday = request.Settings.Schedule.Monday,
                                    Tuesday = request.Settings.Schedule.Tuesday,
                                    Wednesday = request.Settings.Schedule.Wednesday,
                                    Thursday = request.Settings.Schedule.Thursday,
                                    Friday = request.Settings.Schedule.Friday,
                                    Saturday = request.Settings.Schedule.Saturday
                                }
                            },
                        };
                        job = await _dbJobService.CreateAsync(job);
                        if (job != null)
                        {
                            if(await _httpJobService.CreateAsync(device.TenantId, job))
                            {
                                return Ok(job);
                            }
                            return BadRequest("An error occurred while creating the job on the tenant server.");
                        }
                    }
                }
                return BadRequest("An error occurred while creating the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPut("")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateAsync([FromBody] JobUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(request.Id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId == null)
                {
                    return NotFound("Device not found.");
                }
                Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (request.Settings != null && job.Settings != null)
                {
                    NASServer? nasServer = await _dbNASServerService.GetByBackupServerIdAsync(device.TenantId, request.Settings.BackupServerId);
                    if (nasServer == null)
                    {
                        return NotFound("NAS Server not found.");
                    }
                    if (request.Settings.Schedule != null && job.Settings.Schedule != null)
                    {
                        job.Name = request.Name;
                        //job.Settings.BackupServerId = request.Settings.BackupServerId; #disable changing backup server
                        job.Settings.Type = request.Settings.Type;
                        job.Settings.StartTime = request.Settings.StartTime;
                        job.Settings.EndTime = request.Settings.EndTime;
                        job.Settings.UpdateInterval = request.Settings.UpdateInterval;
                        job.Settings.retryCount = request.Settings.retryCount;
                        job.Settings.Sampling = request.Settings.Sampling;
                        job.Settings.Retention = request.Settings.Retention;
                        job.Settings.Schedule.Sunday = request.Settings.Schedule.Sunday;
                        job.Settings.Schedule.Monday = request.Settings.Schedule.Monday;
                        job.Settings.Schedule.Tuesday = request.Settings.Schedule.Tuesday;
                        job.Settings.Schedule.Wednesday = request.Settings.Schedule.Wednesday;
                        job.Settings.Schedule.Thursday = request.Settings.Schedule.Thursday;
                        job.Settings.Schedule.Friday = request.Settings.Schedule.Friday;
                        job.Settings.Schedule.Saturday = request.Settings.Schedule.Saturday;
                        job = await _dbJobService.UpdateAsync(job);
                        if (job != null)
                        {
                            if (job.DeviceId != null)
                            {
                                device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                                if (device != null)
                                {
                                    if (await _httpJobService.UpdateAsync(device.TenantId, job))
                                    {
                                        return Ok(job);
                                    }
                                    return BadRequest("An error occurred while updating the job on the tenant server.");
                                }
                            }
                        }
                    }
                }
                return BadRequest("An error occurred while updating the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPut("Status")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] JobUpdateStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(request.Id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                if (!Enum.IsDefined(typeof(DeviceJobStatus), request.Status))
                {
                    return BadRequest("Invalid Job Status.");
                }
                job.Status = request.Status;
                job = await _dbJobService.UpdateAsync(job);
                if (job != null)
                {
                    return Ok(job);
                }
                return BadRequest("An error occurred while updating the job status.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpDelete("{id}")]
        [HasPermission("Job.Delete.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId == null)
                {
                    return NotFound("Device not found.");
                }
                Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                if (device != null)
                {
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    if (await _httpJobService.DeleteAsync(device.TenantId, device.DeviceInfo.ClientId, job.JobId))
                    {
                        if (await _dbJobService.DeleteAsync(id))
                        {
                            return Ok("Job deleted successfully.");
                        }
                        return BadRequest("An error occurred while deleting the job.");
                    }
                    return BadRequest("An error occurred while deleting the job on the tenant server.");
                }
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("{id}")]
        [HasPermission("Job.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job != null)
                {
                    Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                    if (device == null)
                    {
                        return NotFound("Device not found.");
                    }
                    // Authentication check using roles + permissions
                    if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                    {
                        return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                    }
                    // --- End of authentication check ---
                    return Ok(job);
                }
                return NotFound("Job not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("")]
        [HasPermission("Job.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                var tenantIds = await _authService.GetUserAccessibleTenantsAsync(Request.Headers["Authorization"].ToString());
                if (tenantIds == null)
                {
                    return new CustomForbidResult("User does not have any tenant permissions");
                }
                List<DeviceJob>? jobs = new List<DeviceJob>();
                foreach (var tenantId in tenantIds)
                {
                    if (tenantId != null)
                    {
                        var tenantJobs = await _dbJobService.GetAllByTenantIdAsync((int)tenantId);
                        if (tenantJobs != null)
                        {
                            jobs.AddRange(tenantJobs);
                        }
                    }
                }
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return Ok(jobs.Distinct());
                    }
                }
                return NotFound("No jobs found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        [HasPermission("Job.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceJob>? jobs = await _dbJobService.GetAllByDeviceIdAsync(deviceId);
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return Ok(jobs);
                    }
                }
                return NotFound("No jobs found for the device.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("By-Tenant/{tenantId}")]
        [HasPermission("Job.Get.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Authentication check using roles + permissions
                if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), tenantId))
                {
                    return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                }
                // --- End of authentication check ---
                ICollection<DeviceJob>? jobs = await _dbJobService.GetAllByTenantIdAsync(tenantId);
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return Ok(jobs);
                    }
                }
                return NotFound("No jobs found for the tenant.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPost("{id}/Start")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> StartAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId != null)
                {
                    Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                    if (device != null)
                    {
                        // Authentication check using roles + permissions
                        if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                        {
                            return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                        }
                        // --- End of authentication check ---
                        if (await _httpJobService.StartAsync(device.TenantId, device.DeviceInfo.ClientId))
                        {
                            return Ok("Job started successfully.");
                        }
                        return BadRequest("An error occurred while starting the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while starting the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPost("{id}/Pause")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> PauseAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId != null)
                {
                    Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                    if (device != null)
                    {
                        // Authentication check using roles + permissions
                        if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                        {
                            return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                        }
                        // --- End of authentication check ---
                        if (await _httpJobService.PauseAsync(device.TenantId, device.DeviceInfo.ClientId))
                        {
                            return Ok("Job paused successfully.");
                        }
                        return BadRequest("An error occurred while pausing the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while pausing the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPost("{id}/Resume")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> ResumeAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId != null)
                {
                    Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                    if (device != null)
                    {
                        // Authentication check using roles + permissions
                        if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                        {
                            return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                        }
                        // --- End of authentication check ---
                        if (await _httpJobService.ResumeAsync(device.TenantId, device.DeviceInfo.ClientId))
                        {
                            return Ok("Job resumed successfully.");
                        }
                        return BadRequest("An error occurred while resuming the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while resuming the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPost("{id}/Stop")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> StopAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId != null)
                {
                    Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                    if (device != null)
                    {
                        // Authentication check using roles + permissions
                        if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                        {
                            return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                        }
                        // --- End of authentication check ---
                        if (await _httpJobService.StopAsync(device.TenantId, device.DeviceInfo.ClientId))
                        {
                            return Ok("Job stopped successfully.");
                        }
                        return BadRequest("An error occurred while stopping the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while stopping the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpPost("{id}/Refresh")]
        [HasPermission("Job.Update.Permission", PermissionType.Tenant)]
        public async Task<IActionResult> RefreshAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (job.DeviceId != null)
                {
                    Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                    if (device != null)
                    {
                        // Authentication check using roles + permissions
                        if (!await _authService.UserHasPermissionAsync<JobController>(Request.Headers["Authorization"].ToString(), device.TenantId))
                        {
                            return new CustomForbidResult("User do not have access to this feature for the specified tenant");
                        }
                        // --- End of authentication check ---
                        DeviceJobStatus? status = await _httpJobService.GetStatusAsync(device.TenantId, device.DeviceInfo.ClientId, job.JobId);
                        if (status != null)
                        {
                            job.Status = (DeviceJobStatus)status;
                            job = await _dbJobService.UpdateAsync(job);
                            if (job != null)
                            {
                                return Ok("Job status refreshed.");
                            }
                        }
                        return BadRequest("An error occurred while getting the job status from the tenant server.");
                    }
                }
                return BadRequest("An error occurred while getting the job status.");
            }
            return BadRequest("Invalid request.");
        }
    }
}
