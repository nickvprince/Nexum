using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class DashboardController : Controller
    {
        private readonly TenantService _tenantService;

        public DashboardController(TenantService tenantService)
        {
            _tenantService = tenantService;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
