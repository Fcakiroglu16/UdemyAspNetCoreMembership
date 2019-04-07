using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UdemyIdentity.Models;

namespace UdemyIdentity.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<AppUser> userManager { get; }

        public AdminController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(userManager.Users.ToList());
        }
    }
}