﻿using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
