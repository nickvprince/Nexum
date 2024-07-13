using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;

namespace App.Controllers
{
    public class AlertController : Controller
    {
        private readonly AlertService _alertService;

        public AlertController(AlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Table()
        {
            ICollection<DeviceAlert>? alerts = await _alertService.GetAllAsync();
            return PartialView("_AlertTablePartial", alerts);
        }
    }
}
