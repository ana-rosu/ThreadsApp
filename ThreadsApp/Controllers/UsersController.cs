using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult ShowProfile(string id)
        {
            ApplicationUser user = db.Users.Include("Posts")
                                           .Include("Reposts")
                                           .Include("Followers")
                                           .Include("Followings")
                                           .Where(u => u.Id == id)
                                           .FirstOrDefault();

            user.ProfilePicture ??= "/images/profile/default.png";
    
            string requestStatus = db.Follows
                                   .Where(f => f.FollowerId == _userManager.GetUserId(User) && f.FollowingId == id)
                                   .Select(f => f.Status)
                                   .FirstOrDefault();
            if (requestStatus != null)
            {
                ViewBag.RequestStatus = requestStatus;
            }
            int followingCount = db.Follows
                                .Where(f => f.FollowerId == id) 
                                .Count(f => f.Status == "Following");

            int followersCount = db.Follows
                                .Where(f => f.FollowingId == id)
                                .Count(f => f.Status == "Following");
            ViewBag.FollowingCount = followingCount;
            ViewBag.FollowersCount = followersCount;
            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.Posts = user.Posts;
            ViewBag.Reposts = user.Reposts;
            ViewBag.IsAdmin = User.IsInRole("Admin");
            SetViewRights(id);
            return View(user);
        }
        [Authorize(Roles = "User,Admin")]
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

            ViewBag.Users = paginatedUsers.ToList(); ;


            return View();
        }


        //conditions to view the profile of an user
        private void SetViewRights(string userId)
        {
            ViewBag.SeeContent = false;
            bool isPublic = db.Users.Find(userId).AccountPrivacy == "Public";
            string currentUserId = _userManager.GetUserId(User);

            if (User.IsInRole("Admin") || isPublic || userId == _userManager.GetUserId(User) || db.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == userId && f.Status == "Following"))
            {
                ViewBag.SeeContent = true;
            }
        }
    }
}
