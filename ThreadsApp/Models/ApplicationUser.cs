using Microsoft.AspNetCore.Identity;

namespace ThreadsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Like>? Likes { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Repost>? Reposts { get; set; }
    }

}
