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

    }
}
