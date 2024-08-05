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
    /// <summary>
    /// Controller for managing jobs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class JobController : ControllerBase
    {
        private readonly IDbJobService _dbJobService;
        private readonly ITenantServerAPIJobService _httpJobService;
        private readonly ITenantServerAPIDeviceService _httpDeviceService;
        private readonly IDbDeviceService _dbDeviceService;
        private readonly IDbNASServerService _dbNASServerService;
        private readonly IAPIAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobController"/> class.
        /// </summary>
        /// <param name="dbJobService">The job service.</param>
        /// <param name="httpJobService">The tenant server job service.</param>
        /// <param name="dbDeviceService">The device service.</param>
        /// <param name="dbNASServerService">The NAS server service.</param>
        /// <param name="authService">The authentication service.</param>
        public JobController(IDbJobService dbJobService, ITenantServerAPIJobService httpJobService,
            ITenantServerAPIDeviceService httpDeviceService, IDbDeviceService dbDeviceService, 
            IDbNASServerService dbNASServerService, IAPIAuthService authService)
        {
            _dbJobService = dbJobService;
            _httpJobService = httpJobService;
            _httpDeviceService = httpDeviceService;
            _dbDeviceService = dbDeviceService;
            _dbNASServerService = dbNASServerService;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new job.
        /// </summary>
        /// <param name="request">The job create request.</param>
        /// <returns>An action result containing the created job.</returns>
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
                                RetryCount = request.Settings.RetryCount,
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
                            await _dbJobService.DeleteAsync(job.Id);
                            ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                            if (devices != null)
                            {
                                Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                                if (server != null)
                                {
                                    if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                    {
                                        foreach (var dev in devices)
                                        {
                                            dev.Status = DeviceStatus.Offline;
                                            await _dbDeviceService.UpdateAsync(dev);
                                        }
                                    }
                                }
                            }
                            return BadRequest("An error occurred while creating the job on the tenant server.");
                        }
                    }
                }
                return BadRequest("An error occurred while creating the job.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Updates an existing job.
        /// </summary>
        /// <param name="request">The job update request.</param>
        /// <returns>An action result containing the updated job.</returns>
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
                        job.Settings.BackupServerId = request.Settings.BackupServerId;
                        job.Settings.Type = request.Settings.Type;
                        job.Settings.StartTime = request.Settings.StartTime;
                        job.Settings.EndTime = request.Settings.EndTime;
                        job.Settings.UpdateInterval = request.Settings.UpdateInterval;
                        job.Settings.RetryCount = request.Settings.RetryCount;
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
                                    ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                                    if (devices != null)
                                    {
                                        Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                                        if (server != null)
                                        {
                                            if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                            {
                                                foreach (var dev in devices)
                                                {
                                                    dev.Status = DeviceStatus.Offline;
                                                    await _dbDeviceService.UpdateAsync(dev);
                                                }
                                            }
                                        }
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

        /// <summary>
        /// Updates the status of an existing job.
        /// </summary>
        /// <param name="request">The job update status request.</param>
        /// <returns>An action result containing the updated job status.</returns>
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

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        /// <returns>An action result indicating the outcome of the deletion.</returns>
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
                        if (await _httpJobService.StopAsync(device.TenantId, device.DeviceInfo.ClientId))
                        {
                            if (await _dbJobService.DeleteAsync(id))
                            {
                                return Ok("Job deleted successfully.");
                            }
                            ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                            if (devices != null)
                            {
                                Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                                if (server != null)
                                {
                                    if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                    {
                                        foreach (var dev in devices)
                                        {
                                            dev.Status = DeviceStatus.Offline;
                                            await _dbDeviceService.UpdateAsync(dev);
                                        }
                                    }
                                }
                            }
                            return BadRequest("An error occurred while deleting the job.");
                        }
                        return BadRequest("An error occurred while stopping the job on the tenant server.");
                    }
                    return BadRequest("An error occurred while deleting the job on the tenant server.");
                }
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to retrieve.</param>
        /// <returns>An action result containing the job.</returns>
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

        /// <summary>
        /// Gets all jobs accessible by the authenticated user.
        /// </summary>
        /// <returns>An action result containing all jobs accessible by the user.</returns>
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

        /// <summary>
        /// Gets all jobs for a specific device.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <returns>An action result containing the jobs for the device.</returns>
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

        /// <summary>
        /// Gets all jobs for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>An action result containing the jobs for the tenant.</returns>
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

        /// <summary>
        /// Starts a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to start.</param>
        /// <returns>An action result indicating the outcome of the start operation.</returns>
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
                        ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                        if (devices != null)
                        {
                            Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                            if (server != null)
                            {
                                if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                {
                                    foreach (var dev in devices)
                                    {
                                        dev.Status = DeviceStatus.Offline;
                                        await _dbDeviceService.UpdateAsync(dev);
                                    }
                                }
                            }
                        }
                        return BadRequest("An error occurred while starting the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while starting the job.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Pauses a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to pause.</param>
        /// <returns>An action result indicating the outcome of the pause operation.</returns>
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
                        ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                        if (devices != null)
                        {
                            Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                            if (server != null)
                            {
                                if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                {
                                    foreach (var dev in devices)
                                    {
                                        dev.Status = DeviceStatus.Offline;
                                        await _dbDeviceService.UpdateAsync(dev);
                                    }
                                }
                            }
                        }
                        return BadRequest("An error occurred while pausing the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while pausing the job.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Resumes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to resume.</param>
        /// <returns>An action result indicating the outcome of the resume operation.</returns>
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
                        ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                        if (devices != null)
                        {
                            Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                            if (server != null)
                            {
                                if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                {
                                    foreach (var dev in devices)
                                    {
                                        dev.Status = DeviceStatus.Offline;
                                        await _dbDeviceService.UpdateAsync(dev);
                                    }
                                }
                            }
                        }
                        return BadRequest("An error occurred while resuming the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while resuming the job.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Stops a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to stop.</param>
        /// <returns>An action result indicating the outcome of the stop operation.</returns>
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
                        ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                        if (devices != null)
                        {
                            Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                            if (server != null)
                            {
                                if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                {
                                    foreach (var dev in devices)
                                    {
                                        dev.Status = DeviceStatus.Offline;
                                        await _dbDeviceService.UpdateAsync(dev);
                                    }
                                }
                            }
                        }
                        return BadRequest("An error occurred while stopping the job on the tenant server.");
                    }
                }
                return BadRequest("An error occurred while stopping the job.");
            }
            return BadRequest("Invalid request.");
        }

        /// <summary>
        /// Refreshes the status of a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to refresh.</param>
        /// <returns>An action result indicating the outcome of the refresh operation.</returns>
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
                        ICollection<Device>? devices = await _dbDeviceService.GetAllByTenantIdAsync(device.TenantId);
                        if (devices != null)
                        {
                            Device? server = devices.Where(d => d.DeviceInfo.Type == DeviceType.Server).FirstOrDefault();
                            if (server != null)
                            {
                                if ((bool)!await _httpDeviceService.ForceDeviceCheckinAsync(device.TenantId, server.DeviceInfo.ClientId))
                                {
                                    foreach (var dev in devices)
                                    {
                                        dev.Status = DeviceStatus.Offline;
                                        await _dbDeviceService.UpdateAsync(dev);
                                    }
                                }
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
