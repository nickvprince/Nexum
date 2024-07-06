using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.WebRequestEntities.JobRequests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1-Web")]
    public class JobController : ControllerBase
    {
        private readonly DbJobService _dbJobService;
        private readonly DbDeviceService _dbDeviceService;

        public JobController(DbJobService dbJobService, DbDeviceService dbDeviceService)
        {
            _dbJobService = dbJobService;
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
                            return Ok(job);
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
                            return Ok(job);
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
                    return Ok();
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

        [HttpGet("By-NASServer/{nasServerId}")]
        public async Task<IActionResult> GetAllByNASServerIdAsync(int nasServerId)
        {
            if (ModelState.IsValid)
            {
                ICollection<DeviceJob>? jobs = await _dbJobService.GetAllByNASServerIdAsync(nasServerId);
                if (jobs != null)
                {
                    if (jobs.Any())
                    {
                        return Ok(jobs);
                    }
                }
                return NotFound("No jobs found for the backup server.");
            }
            return BadRequest("Invalid request.");
        }
    }
}
