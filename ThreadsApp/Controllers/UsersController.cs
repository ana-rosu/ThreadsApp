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
                                           .Include("Followers")
                                           .Include("Followings")
                                           .Where(u => u.Id == id)
                                           .FirstOrDefault();

            

            user.ProfilePicture = string.IsNullOrEmpty(user.ProfilePicture)
                             ? "/images/profile/default.png"
                             : user.ProfilePicture;

    
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

            IQueryable<ApplicationUser> users = db.Users;

            var search = "";


            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();


                List<string> userIds = db.Users.Where( u => u.UserName.Contains(search) 
                                                    || u.FirstName.Contains(search)
                                                    || u.LastName.Contains(search))
                                               .Select(u => u.Id).ToList();

                users = db.Users.Where(user => userIds.Contains(user.Id));


            }

            ViewBag.SearchString = search;

            int _perpage = 4;

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
