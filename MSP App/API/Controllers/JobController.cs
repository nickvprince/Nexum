using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebEntities.Requests.JobRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class JobController : ControllerBase
    {
        private readonly DbJobService _dbJobService;
        private readonly HTTPJobService _httpJobService;
        private readonly DbDeviceService _dbDeviceService;

        public JobController(DbJobService dbJobService, HTTPJobService httpJobService, DbDeviceService dbDeviceService)
        {
            _dbJobService = dbJobService;
            _httpJobService = httpJobService;
            _dbDeviceService = dbDeviceService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAsync([FromBody] JobCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                Device? device = await _dbDeviceService.GetAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound("Device not found.");
                }
                if (request.Settings != null)
                {
                    if(request.Settings.Schedule != null)
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
        public async Task<IActionResult> UpdateAsync([FromBody] JobUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(request.Id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (request.Settings != null && job.Settings != null)
                {
                    if (request.Settings.Schedule != null && job.Settings.Schedule != null)
                    {
                        job.Name = request.Name;
                        job.Settings.BackupServerId = request.Settings.BackupServerId;
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
                                Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
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
        public async Task<IActionResult> UpdateStatusAsync([FromBody] JobUpdateStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(request.Id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
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
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (ModelState.IsValid)
            {
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job == null)
                {
                    return NotFound("Job not found.");
                }
                if (await _dbJobService.DeleteAsync(id))
                {
                    if (job.DeviceId != null)
                    {
                        Device? device = await _dbDeviceService.GetAsync((int)job.DeviceId);
                        if (device != null)
                        {
                            if (await _httpJobService.DeleteAsync(device.TenantId, device.DeviceInfo.ClientId, job.Id))
                            {
                                return Ok("Job deleted successfully.");
                            }
                            return BadRequest("An error occurred while deleting the job on the tenant server.");
                        }
                    }
                    return Ok("Job deleted successfully.");
                }
                return BadRequest("An error occurred while deleting the job.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            if (ModelState.IsValid)
            {
                // Update this to get the job from the tenant server
                // Then update the job in the database
                // Then, below
                DeviceJob? job = await _dbJobService.GetAsync(id);
                if (job != null)
                {
                    return Ok(job);
                }
                return NotFound("Job not found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllAsync()
        {
            if (ModelState.IsValid)
            {
                // Update this to get all jobs from the tenant server
                // Then update the jobs in the database
                // Then, below
                ICollection<DeviceJob>? jobs = await _dbJobService.GetAllAsync();
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return Ok(jobs);
                    }
                }
                return NotFound("No jobs found.");
            }
            return BadRequest("Invalid request.");
        }

        [HttpGet("By-Device/{deviceId}")]
        public async Task<IActionResult> GetAllByDeviceIdAsync(int deviceId)
        {
            if (ModelState.IsValid)
            {
                // Update this to get all jobs from the tenant server
                // Then update the jobs in the database
                // Then, below
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
        public async Task<IActionResult> GetAllByTenantIdAsync(int tenantId)
        {
            if (ModelState.IsValid)
            {
                // Update this to get all jobs from the tenant server
                // Then update the jobs in the database
                // Then, below
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
                        if (await _httpJobService.StartAsync(device.TenantId, job.Id))
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
                        if (await _httpJobService.PauseAsync(device.TenantId, job.Id))
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
                        if (await _httpJobService.ResumeAsync(device.TenantId, job.Id))
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

        [HttpGet("{id}/Stop")]
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
                        if (await _httpJobService.StopAsync(device.TenantId, job.Id))
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

        [HttpGet("{id}/Refresh")]
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
                        DeviceJobStatus? status = await _httpJobService.GetStatusAsync(device.TenantId, device.DeviceInfo.ClientId, job.Id);
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
