using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ThreadsApp.Data;
using ThreadsApp.Models;

namespace ThreadsApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        
        // adding a comment associated to a post in db
        [HttpPost]
        public IActionResult New (Comment comm, int Page)
        {
            comm.Date = DateTime.Now;
            comm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                _db.Comments.Add(comm);
                _db.SaveChanges();

                return Redirect($"/Posts/Index?page={Page}");
            }
            else
            {
                return Redirect($"/Posts/Index?page={Page}");
            }

        }

        // deleting a comment associated to a post from db
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id, int Page)
        {
            Comment comm = _db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                _db.Comments.Remove(comm);
                _db.SaveChanges();
                return Redirect($"/Posts/Index?page={Page}");
            }

            else
            {
                TempData["message"] = "You don't have the right to delete this comment.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }
        }

        // displaying the form to edit a comment 
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = _db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                ViewBag.Page = HttpContext.Request.Query["page"];
                return View(comm);              
            }

            else
            {
                TempData["message"] = "You don't have the right to edit this comment.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }
        }
        // processing the form submission by saving the edits of the comment in db
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, int Page, Comment requestComment)
        {
            Comment comm = _db.Comments.Find(id);
            comm.Date = DateTime.Now;

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestComment.Content;

                    _db.SaveChanges();
                    return Redirect($"/Posts/Index?page={Page}");
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["message"] = "You don't have the right to edit this comment.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }
        }
    }
}