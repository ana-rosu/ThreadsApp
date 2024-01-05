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
            Group group = _db.Groups
                        .Include(g => g.Posts)
                            .ThenInclude(p => p.User)
                        .Include(g => g.User)
                        .Include(g => g.Posts)
                            .ThenInclude(p => p.Comments)
                                .ThenInclude(c => c.User)
                        .Where(grp => grp.Id == id)
                        .FirstOrDefault();


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            string requestStatus = _db.UserGroups
                                    .Where(ug => ug.UserId == _userManager.GetUserId(User) && ug.GroupId == id)
                                    .Select(ug => ug.MembershipStatus)
                                    .FirstOrDefault();
            if (requestStatus != null)
            {
                ViewBag.RequestStatus = requestStatus;
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
                //UserGroup userGroup = new UserGroup
                //{
                //    UserId = group.UserId,
                //    GroupId = group.Id,
                //    MembershipStatus = "Admin",
                //};
                //_db.UserGroups.Add(userGroup);
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
        [Authorize(Roles = "User,Admin")]
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
        [Authorize(Roles = "User,Admin")]
        public IActionResult JoinGroup(int id)
        {
            Group group = _db.Groups.Find(id);

            var CurrentUserId = _userManager.GetUserId(User);
            bool isOwner = CurrentUserId == group.UserId;

            if (isOwner)
            {
                TempData["message"] = "You can not join a group that you own.";
                TempData["messageType"] = "alert-danger";
            }
            else
            {
                var pendingRequest = _db.UserGroups.Any(ug => ug.GroupId == id && ug.UserId == CurrentUserId && ug.MembershipStatus == "Pending");

                if (pendingRequest)
                {
                    TempData["message"] = "You already have a pending request to join the group.";
                    TempData["messageType"] = "alert-danger";
                    
                }
                else
                {
                    bool isMember = _db.UserGroups.Any(ug => ug.GroupId == id && ug.UserId == CurrentUserId && ug.MembershipStatus == "Member");

                    if (!isMember)
                    {
                        var userGroup = new UserGroup
                        {
                            UserId = CurrentUserId,
                            GroupId = id,
                            MembershipStatus = "Pending"
                        };

                        _db.UserGroups.Add(userGroup);

                        _db.SaveChanges();

                        TempData["message"] = "You have successfully requested to join the group and is pending approval by the group owner.";
                        TempData["messageType"] = "alert-success";
                        
                    }
                    else
                    {
                        TempData["message"] = "You are already a member of the group.";
                        TempData["messageType"] = "alert-danger";
                    
                    }
                }

            }
            return RedirectToAction("Show", new { id = id });
        }
        // displaying to the owner of the group the list with all the users that requested to join
        [Authorize(Roles="User,Admin")]
        public IActionResult ManageRequests(int id)
        {
            var ownerId = _db.Groups.Where(g => g.Id == id).Select(g => g.UserId).FirstOrDefault();
            if (ownerId == _userManager.GetUserId(User))
            {
                var pendingRequests = _db.UserGroups
                                      .Where(ug => ug.GroupId == id && ug.MembershipStatus == "Pending")
                                      .Include(ug => ug.User)
                                      .ToList();
                if (pendingRequests.Any())
                {
                    return View(pendingRequests);
                }
                else
                {
                    TempData["message"] = "There are no more pending requests!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", new { id = id });
                }
            }
            else
            {
                TempData["message"] = "You can not manage requests for a group you are not an owner of";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new {id = id});
            }
        }
        // processing the accepting a request in db
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult AcceptRequest(int userGroupId)
        {
            var userGroup = _db.UserGroups
                            .Where(ug => ug.Id == userGroupId)
                            .Include(ug => ug.Group)
                            .FirstOrDefault();
            if (userGroup != null)
            {
                userGroup.MembershipStatus = "Member";
                if (userGroup.Group != null)
                {
                    userGroup.Group.MemberCount += 1;
                }

                _db.SaveChanges();

                TempData["message"] = "Membership request accepted successfully!";
                TempData["messageType"] = "alert-success";
            }

            return RedirectToAction("ManageRequests", new { id = userGroup.GroupId });
        }
    
        // processing the refusing of a request in db
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult RefuseRequest(int userGroupId)
        {
            var userGroup = _db.UserGroups.Find(userGroupId);
            var CurrentGroupId = userGroup.GroupId;
            _db.UserGroups.Remove(userGroup);
            _db.SaveChanges();

            TempData["message"] = "Membership request refused!";
            TempData["messageType"] = "alert-danger";

            return RedirectToAction("ManageRequests", new { id = CurrentGroupId });
        }

        [HttpPost]
        public IActionResult ShowMembers(int groupId)
        {   
            List<ApplicationUser> members = _db.UserGroups
                                        .Where(u => u.GroupId == groupId)
                                        .Include(u => u.User)
                                        .Select(u => u.User)
                                        .ToList();

            Group group = _db.Groups
                            .Include(g => g.User)
                            .FirstOrDefault(g => g.Id == groupId);

            ApplicationUser admin = group.User;

            members.Insert(0, admin);
            return View(members);
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

            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == group?.UserId || _db.UserGroups.Any(ug => ug.UserId == _userManager.GetUserId(User) && ug.GroupId == group.Id && ug.MembershipStatus == "Member")){
                ViewBag.SeeContent = true;
            }
        }
    }
}