using System.ComponentModel.DataAnnotations;

namespace ThreadsApp.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The title is mandatory!")]
        [MaxLength(50, ErrorMessage = "The maximum length of the title is 50 characters")]
        public string Title { get; set; }
        public int MemberCount {  get; set; }
        public DateTime Date { get; set; }
        //atribut pt group image
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        // m:m relationship between users and groups
        public virtual ICollection<UserGroup>? UserGroups { get; set; }
    }
}
