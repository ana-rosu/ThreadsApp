using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ThreadsApp.Data;
using ThreadsApp.Models;

namespace ThreadsApp.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class RepostsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RepostsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult New(Repost repost, int? page)
        {
            repost.Date = DateTime.Now;
            repost.UserId = _userManager.GetUserId(User);
            
            if (ModelState.IsValid)
            {
                db.Reposts.Add(repost);
                db.SaveChanges();
                TempData["message"] = "Repost was successfully added";
                TempData["messageType"] = "alert-success";

                if (page != null)
                {
                    return Redirect($"/Posts/Index?page={page}");
                }
                else
                {
                    return RedirectToAction("Index", "Posts");
                }
            }
            else
            {
                TempData["message"] = "Repost couldn't be added, try again.";
                TempData["messageType"] = "alert-danger";
            }

            return Redirect($"/Posts/Index?page={page}");

        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id, int? page)
        {
            Repost repost = db.Reposts.Find(id);

            if (repost.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reposts.Remove(repost);
                db.SaveChanges();

                TempData["message"] = "Repost was successfully added";
                TempData["messageType"] = "alert-success";

                if (page != null)
                {
                    return Redirect($"/Posts/Index?page={page}");
                }
                else
                {
                    return RedirectToAction("Index", "Posts");
                }
            }
            else
            {
                TempData["message"] = "You don't have the right to delete this repost.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index", "Posts");

        }

        [Authorize(Roles = "User")]
        public IActionResult Edit(int id)
        {
            Repost repost = db.Reposts.Where(r => r.Id == id)
                                      .First();

            if (repost.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                ViewBag.Page = HttpContext.Request.Query["page"];
                return View(repost);
            }

            else
            {
                return RedirectToAction("Index", "Posts");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult Edit(int id, int? page, Repost requestRepost)
        {
            Repost repost = db.Reposts.Find(id);

            if (repost.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    repost.Content = requestRepost.Content;

                    repost.Date = DateTime.Now;

                    db.SaveChanges();
                    
                    TempData["message"] = "Repost was successfully edited.";
                    TempData["messageType"] = "alert-success";

                    if (page != null)
                    {
                        return Redirect($"/Posts/Index?page={page}");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Posts");
                    }
                }
                else
                {
                    return View(requestRepost);
                }
            }
            else
            {
                TempData["message"] = "You don't have the right to edit this repost.";
                TempData["messageType"] = "alert-danger";

                return RedirectToAction("Index", "Posts");
            }
        }



    }
}
