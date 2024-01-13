using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreadsApp.Data;
using ThreadsApp.Models;

namespace ThreadsApp.Controllers
{
    public class PostsController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PostsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index(int? page)
        {
            int _perpage = 5;
            
            var posts = db.Posts.Include("Comments").Include("User").Include("PostReposts").Include("Likes")
                                .Where(p => p.GroupId == null)
                                .OrderByDescending(p => p.Date)
                                .ToList();

            SetAccessRights();

            foreach (var post in posts)
            {
                post.LikesCount = post.Likes?.Count ?? 0;
                post.FormattedDate = ToRelativeDate(post.Date);
                ViewData[$"UserLiked_{post.Id}"] = post.Likes.Any(l => l.UserId == _userManager.GetUserId(User));
            }


            ViewBag.Posts = posts;


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            int totalItems = posts.Count();

            var currentPage = page ?? Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perpage;
            }

            var paginatedPosts = posts.Skip(offset).Take(_perpage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perpage);

            ViewBag.Posts = paginatedPosts;
            ViewBag.currentPage = currentPage;
            return View();
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Post post = db.Posts.Include("Comments")
                              .Include("User")
                              .Include("Comments.User")
                              .Include("Likes")
                              .Where(p => p.Id == id)
                              .First();

            post.LikesCount = post.Likes?.Count ?? 0;
            post.FormattedDate = ToRelativeDate(post.Date);

            SetAccessRights();

            return View(post);
        }

        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Post post, int? groupId)
        {
            post.Date = DateTime.Now;
            post.FormattedDate = ToRelativeDate(post.Date);

            post.UserId = _userManager.GetUserId(User);

            if (groupId != null)
            {
                post.GroupId = groupId;
            }

            post.LikesCount = 0;

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                TempData["message"] = "Post was successfully added";
                TempData["messageType"] = "alert-success";

                if (post.GroupId != null)
                {
                    return Redirect("/Groups/Show/" + groupId);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            else
            {
                return View(post);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Include("Comments")
                                .Include("Likes")
                                .Include("PostReposts")
                                .Where(p => p.Id == id)
                                .First();

            if (post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Posts.Remove(post);
                db.SaveChanges();
                TempData["message"] = "Post deleted successfully";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Post cannot be deleted because you are not the owner";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {

            Post post = db.Posts.Where(p => p.Id == id)
                                .First();


            if (post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "Post cannot be edited because you are not the owner";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Post requestPost)
        {
            var sanitizer = new HtmlSanitizer();

            Post post = db.Posts.Find(id);


            if (ModelState.IsValid)
            {
                if (post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {

                    requestPost.Content = sanitizer.Sanitize(requestPost.Content);

                    post.Content = requestPost.Content;

                    post.Date = DateTime.Now;
                    post.FormattedDate = ToRelativeDate(post.Date);

                    TempData["message"] = "Post was successfully edited";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Post cannot be edited because you are not the owner";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestPost);
            }
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

        [HttpPost]
        public IActionResult Like(int postId, int currentPage)
        {
            Post post = db.Posts.Find(postId);

            if (!db.Likes.Any(p => p.PostId == postId && p.UserId == _userManager.GetUserId(User)))
            {
                var like = new Like
                {
                    PostId = postId,
                    UserId = _userManager.GetUserId(User),
                };

                db.Likes.Add(like);
                db.SaveChanges();
            }
            else
            {
                Like like = db.Likes.Where(l => l.PostId == postId && l.UserId == _userManager.GetUserId(User)).First();

                db.Likes.Remove(like);
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { page = currentPage});
        }
    }
}
