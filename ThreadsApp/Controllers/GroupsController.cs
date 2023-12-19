using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreadsApp.Data;
using ThreadsApp.Models;
using Group = ThreadsApp.Models.Group;

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
        //displaying all groups from db
        public IActionResult Index()
        {
            int _perPage = 3;
            var groups = _db.Groups.Include("User").OrderBy(g => g.Date);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
                ViewBag.Alert = TempData["messageType"];
            }
            // being a variable no of groups we check each time
            int totalItems = groups.Count();
            // /Groups/Index?page=val
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0; //the offset of each page is equal to the no of groups shown before
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedGroups = groups.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Groups = paginatedGroups;
            return View();
        }
        //displaying a group
        public IActionResult Show(int id)
        {
            Group group = _db.Groups.Include("Posts").Include("User")
                                         .Where(grp => grp.Id == id)
                                         .First();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            SetAccessRights();
            SetViewRights(id);
            return View(group);

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
        // displaying the form to edit a group
        [Authorize(Roles = "User,Admin")]
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
        // processing the form submission by saving the edits in the database
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
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
        // processing the deletion of the group in the database
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Group group = _db.Groups.Include("Posts").Where(grp => grp.Id == id).First();

            if (group.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                _db.Groups.Remove(group);
                _db.SaveChanges();
                TempData["message"] = "The group was successfully deleted!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You can not delete a group you don't own!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        // processing in db the joining to a group 
        [HttpPost]
        public IActionResult JoinGroup(int id)
        {
            var CurrentUserId = _userManager.GetUserId(User);
            bool isMember = _db.UserGroups.Any(ug => ug.GroupId == id && ug.UserId == CurrentUserId);

            if (!isMember)
            {
                var userGroup = new UserGroup
                {
                    UserId = CurrentUserId,
                    GroupId = id
                };

                _db.UserGroups.Add(userGroup);
                _db.SaveChanges();

                TempData["message"] = "You have successfully joined the group!";
            }
            else
            {
                TempData["message"] = "You are already a member of the group.";
            }

            return RedirectToAction("Show");
        }
        // conditions to show edit and delete buttons 
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }
        // conditions to view the content of a group
        private void SetViewRights(int id)
        {   Group group = _db.Groups.Find(id);
            ViewBag.SeeContent = false;

            if (User.IsInRole("Admin") || _db.UserGroups.Any(ug => ug.UserId == _userManager.GetUserId(User) && ug.GroupId == group.Id)){
                ViewBag.SeeContent = true;
            }
        }
    }
}
//var members = _db.UserGroups
//            .Where(ug => ug.GroupId == group.Id)
//            .Select(ug => ug.User)
//            .ToList();
//Console.WriteLine("Members of the group:");
//foreach (var member in members)
//{
//    Console.WriteLine($"User Id: {member.Id}, UserName: {member.UserName}");
//    // Add other properties as needed
//}