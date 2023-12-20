using Microsoft.AspNetCore.Identity;

namespace ThreadsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName {  get; set; }
        public string? Bio {  get; set; }
        public string? AccountPrivacy {  get; set; }
        public string? ProfilePicture {  get; set; }
        public virtual ICollection<Follow>? Followers { get; set; }
        public virtual ICollection<Follow>? Followings { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Repost>? Reposts { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Group>? Groups { get; set; }
    }

}
