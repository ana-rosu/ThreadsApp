using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThreadsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName {  get; set; }
        public string? Bio {  get; set; }
        public string? AccountPrivacy {  get; set; }
        public static List<SelectListItem> PrivacyOptions { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Public", Text = "Public" },
            new SelectListItem { Value = "Private", Text = "Private" },
        };
        [NotMapped] 
        public IFormFile? Image { get; set; } 
        public string? ProfilePicture {  get; set; }
        [NotMapped] // exclude from database mapping
        public IFormFile? Image { get; set; } // property for group image
        public string? ImagePath { get; set; } // path to the uploaded image
        public virtual ICollection<Follow>? Followers { get; set; }
        public virtual ICollection<Follow>? Followings { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Repost>? Reposts { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Group>? Groups { get; set; }
        public virtual ICollection<UserGroup>? UserGroups { get; set; }
    }

}
