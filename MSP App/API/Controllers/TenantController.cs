using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.DbServices;
using SharedComponents.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly DbTenantService _dbTenantService;

        public TenantController(DbTenantService dbTenantService)
        {
            _dbTenantService = dbTenantService;
        }

        [HttpPost("Create")]
        public IActionResult CreateTenant([FromBody] object tenant)
        {
            //Create the permission
            return Ok($"Tenant created successfully.");
        }

        [HttpPut("Update")]
        public IActionResult UpdateTenant([FromBody] object tenant)
        {
            //Update the permission
            return Ok($"Tenant updated successfully.");
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeleteTenant(string id)
        {
            //Delete the permission
            return Ok($"Tenant deleted successfully.");
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetTenant(string id)
        {
            //Get the permission
            return Ok($"Retrieved tenant successfully.");
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetTenantsAsync()
        {
            //Get the permissions
            List<Tenant> tenants = await _dbTenantService.GetAllAsync();

            if (tenants.Any())
            {
                var response = new
                {
                    data = tenants,
                    message = $"Retrieved tenants successfully."
                };
                return Ok(response);
            }
            return NotFound(new { message = "No tenants found." });
        }
    }
}
