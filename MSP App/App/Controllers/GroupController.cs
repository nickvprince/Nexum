using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class GroupController : Controller
    {
        private readonly GroupService _groupService;

        public GroupController(GroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            GroupViewModel groupViewModel = new GroupViewModel
            {
                Groups = await _groupService.GetAllAsync()
            };

            return View(groupViewModel);
        }
    }
}
