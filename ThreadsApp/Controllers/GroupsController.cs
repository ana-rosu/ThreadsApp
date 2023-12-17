using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Drawing2D;
using ThreadsApp.Data;
using ThreadsApp.Models;

namespace ThreadsApp.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        // displaying the form to create a new group
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Group group = new Group();
            return View(group);
        }

        // processing the form submission by adding group in database
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult New(Group group)
        {
            // populating additional fields before saving
            // (the other ones will be automatically populated with the values entered by the user in the form (if they are valid) = model binding)
            group.Date = DateTime.Now;
            group.MemberCount = 1;
            group.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                _db.Groups.Add(group);
                _db.SaveChanges();

                TempData["message"] = "The group was created successfully!";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Index");
            }
            else
            {
                return View(group);
            }
        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id)
        {
            Group group = _db.Groups.Find(id);
            if (group.UserId ==  _userManager.GetUserId(User) || User.IsInRole("Admin")) {
                return View(group);
            }
            else
            {
                TempData["message"] = "You can not modify a group you don't own!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Group requestedGroup)
        {
            Group group = _db.Groups.Find(id);

            if (ModelState.IsValid)
            {
                if (group.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")){
                    group.Title = requestedGroup.Title;
                    TempData["message"] = "The group was successfully modified!";
                    TempData["messageType"] = "alert-success";
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You can not modify a group you don't own!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestedGroup);
            }
        }
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Group group = _db.Groups.Find(id);
            // no access rights checked because I only call this method from client through a button which is shown only if the creater of the group or the admin wants to delete the group
            _db.Groups.Remove(group);
            _db.SaveChanges();
            TempData["message"] = "Articolul a fost sters";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
            
        }

        // conditions to show edit and delete buttons 
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }
    }
}
