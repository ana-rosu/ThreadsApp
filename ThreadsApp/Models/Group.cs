using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped] // exclude from database mapping
        public IFormFile? Image { get; set; } // property for group image
        public string? ImagePath { get; set; } // path to the uploaded image
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        // m:m relationship between users and groups
        public virtual ICollection<UserGroup>? UserGroups { get; set; }
    }
}
