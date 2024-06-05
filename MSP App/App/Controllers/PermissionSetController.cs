using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class PermissionSetController : Controller
    {
        private readonly UserPermissionSetService _userPermissionSetService;

        public PermissionSetController(UserPermissionSetService userPermissionSetService)
        {
            _userPermissionSetService = userPermissionSetService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            UserPermissionSetViewModel userPermissionSetViewModel = new UserPermissionSetViewModel
            {
                UserPermissionSets = await _userPermissionSetService.GetAllAsync()
            };
            return View(userPermissionSetViewModel);
        }
    }
}
