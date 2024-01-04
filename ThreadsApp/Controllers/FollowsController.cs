using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing;
using ThreadsApp.Data;
using ThreadsApp.Models;

namespace ThreadsApp.Controllers
{
    public class FollowsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public FollowsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // processing in db the follow request
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult FollowUnfollowUser(string userId)
        {
            var CurrentUserId = _userManager.GetUserId(User);
            var pendingRequest = _db.Follows
                                .FirstOrDefault(f => f.FollowingId == userId && f.FollowerId == CurrentUserId && f.Status == "Pending");

            if (pendingRequest != null)
            {   
                _db.Follows.Remove(pendingRequest);
                _db.SaveChanges();
                TempData["message"] = "You withdrew the follow request successfully.";
                TempData["messageType"] = "alert-danger";

            }
            else
            {
                var isFollower = _db.Follows.FirstOrDefault(f => f.FollowingId == userId && f.FollowerId == CurrentUserId && f.Status == "Following");

                if (isFollower != null)
                {
                    _db.Follows.Remove(isFollower);
                    _db.SaveChanges();
                    TempData["message"] = "You have successfully unfollowed this user.";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    var follow = new Follow
                    {
                        FollowerId = CurrentUserId,
                        FollowingId = userId,
                        Status = "Pending"
                    };

                    _db.Follows.Add(follow);
                    
                    _db.SaveChanges();

                    TempData["message"] = "You have successfully requested to follow this user.";
                    TempData["messageType"] = "alert-success";
                }
            }

            return RedirectToAction("ShowProfile", "Users", new { id = userId });

        }
        // displaying to the user the list with all the users that requested to follow its profile ONLY IF THE PROFILE IS PRIVATE
        [Authorize(Roles = "User,Admin")]
        public IActionResult ManageRequests(string? userId)
        {   
            string CurrentUserId = _userManager.GetUserId(User);
            if (userId == null)
            {   // I call this action from the nav bar (from the current user) with no argument because I don't have the _userManager in the _Layout.cshtml 
                // so I know I have to handle the requests for the current user
                userId = CurrentUserId;
            }
            // protecting routes (in the case this action is not called from the nav bar but rather from the url)
            if (CurrentUserId == userId)
            {
                var pendingRequests = _db.Follows
                                        .Where(f => f.FollowingId == userId && f.Status == "Pending")
                                        .Include(f => f.Follower)
                                        .ToList();
                if (pendingRequests.Any())
                {
                    return View(pendingRequests);
                }
                else
                {
                    TempData["message"] = "You're all caught up! There are no new follow requests!";
                    TempData["messageType"] = "alert-danger";
                    return View(pendingRequests);
                }
            }
            else
            {
                TempData["message"] = "You can not manage follow requests of someone else!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }
        }
        // processing the accepting a request in db
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult AcceptRequest(int followId)
        {
            var followRequest = _db.Follows
                                .Include(f => f.Follower)
                                .Include(f => f.Following)
                                .SingleOrDefault(f => f.Id == followId);

            if (followRequest != null)
            {
                followRequest.Status = "Following";
                ApplicationUser FollowerUser = followRequest.Follower;
                ApplicationUser UserBeingFollowed = followRequest.Following;

                FollowerUser.Followings ??= new List<Follow>();
                UserBeingFollowed.Followers ??= new List<Follow>();

                UserBeingFollowed.Followers.Add(followRequest);
                FollowerUser.Followings.Add(followRequest);
                
                _db.SaveChanges();

                TempData["message"] = "Follow request accepted successfully!";
                TempData["messageType"] = "alert-success";
            }

            return RedirectToAction("ManageRequests", new { id = followRequest.FollowingId});
        }

        // processing the refusing of a request in db
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult RefuseRequest(int followId)
        {
            var followRequest = _db.Follows.Find(followId);

            _db.Follows.Remove(followRequest);
            _db.SaveChanges();

            TempData["message"] = "Follow request refused!";
            TempData["messageType"] = "alert-danger";

            return RedirectToAction("ManageRequests", new { id = followRequest.FollowingId });
        }
        [HttpPost]
        public IActionResult ShowFollowers(string userId)
        {
            Debug.WriteLine("fucking null", userId);
            List<ApplicationUser> followers = _db.Follows
                                                .Where(f => f.FollowingId == userId)
                                                .Include(f => f.Follower)
                                                .Select(f => f.Follower)
                                                .ToList();
            return View(followers);
        }
        [HttpPost]
        public IActionResult ShowFollowings(string userId)
        {
            List<ApplicationUser> followings = _db.Follows
                                                .Where(f => f.FollowerId == userId)
                                                .Include(f => f.Following)
                                                .Select(f => f.Following)
                                                .ToList();

            return View(followings);
        }
    }
}
