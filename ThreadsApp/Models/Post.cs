using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThreadsApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="The content of the post is required")]
        [StringLength(500, ErrorMessage = "The text can't be longer than 500 characters")]
        public string? Content { get; set; }
        

        public DateTime Date { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public int? GroupId { get; set; }
        public virtual Group? Group { get; set; }

        public virtual ICollection<Repost>? Reposts { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }

    }
}
