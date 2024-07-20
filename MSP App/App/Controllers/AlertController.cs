using App.Services;
using Microsoft.AspNetCore.Mvc;
using SharedComponents.Entities;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Services.APIRequestServices.Interfaces;

namespace App.Controllers
{
    public class AlertController : Controller
    {
        private readonly IAPIRequestAlertService _alertService;

        public AlertController(IAPIRequestAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> Table()
        {
            ICollection<DeviceAlert>? alerts = await _alertService.GetAllAsync();
            return PartialView("_AlertTablePartial", alerts);
        }
    }
}
