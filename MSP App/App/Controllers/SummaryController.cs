using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class SummaryController : Controller
    {
        public IActionResult Index()
        {
            return PartialView();
        }
    }
}
