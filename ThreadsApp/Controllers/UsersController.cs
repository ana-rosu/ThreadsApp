using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreadsApp.Data;
using ThreadsApp.Models;

namespace ThreadsApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        public IActionResult ShowProfile(string id)
        {
            ApplicationUser user = db.Users.Include("Posts")
                                           .Include("Reposts")
                                           .Where(u => u.Id == id)
                                           .First();

            user.ProfilePicture = string.IsNullOrEmpty(user.ProfilePicture)
                             ? "/images/profile/default.png"
                             : user.ProfilePicture;

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.Posts = user.Posts;

            return View(user);
        }
    }
}
