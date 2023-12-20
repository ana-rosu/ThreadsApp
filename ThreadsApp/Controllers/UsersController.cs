using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

        [Authorize(Roles = "User,Admin")]
        public IActionResult ShowProfile(string id)
        {
            ApplicationUser user = db.Users.Include("Posts")
                                           .Include("Reposts")
                                           .Where(u => u.Id == id)
                                           .First();

            

            user.ProfilePicture = string.IsNullOrEmpty(user.ProfilePicture)
                             ? "/images/profile/default.png"
                             : user.ProfilePicture;

            if (user.Posts != null)
            {
                foreach (var post in user.Posts)
                {
                    post.LikesCount = post.Likes?.Count ?? 0;
                    post.FormattedDate = ToRelativeDate(post.Date);
                }
            }
            
            ViewBag.CurrentUser = _userManager.GetUserId(User);
                ViewBag.Posts = user.Posts;
                ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        public IActionResult Index()
        {
            int _perpage = 4;

            var users = db.Users;

            foreach ( var user in users )
            {
                user.ProfilePicture = string.IsNullOrEmpty(user.ProfilePicture)
                             ? "/images/profile/default.png"
                             : user.ProfilePicture;
            }

            ViewBag.Users = users;

            int totalItems = users.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perpage;
            }

            var paginatedUsers = users.Skip(offset).Take(_perpage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perpage);

            ViewBag.Users = paginatedUsers;


            return View();
        }


        [NonAction]
        public string ToRelativeDate(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return string.Format("{0} seconds ago", timeSpan.Seconds);

            if (timeSpan <= TimeSpan.FromMinutes(60))
                return timeSpan.Minutes > 1 ? String.Format("about {0} minutes ago", timeSpan.Minutes) : "about a minute ago";

            if (timeSpan <= TimeSpan.FromHours(24))
                return timeSpan.Hours > 1 ? String.Format("about {0} hours ago", timeSpan.Hours) : "about an hour ago";

            if (timeSpan <= TimeSpan.FromDays(30))
                return timeSpan.Days > 1 ? String.Format("about {0} days ago", timeSpan.Days) : "yesterday";

            if (timeSpan <= TimeSpan.FromDays(365))
                return timeSpan.Days > 30 ? String.Format("about {0} months ago", timeSpan.Days / 30) : "about a month ago";

            return timeSpan.Days > 365 ? String.Format("about {0} years ago", timeSpan.Days / 365) : "about a year ago";
        }
    }
}
