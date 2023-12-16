using System.ComponentModel.DataAnnotations;

namespace ThreadsApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="The content of the comment is required")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public int? PostId { get; set; }
        public virtual Post? Post { get; set; }

    }
}
