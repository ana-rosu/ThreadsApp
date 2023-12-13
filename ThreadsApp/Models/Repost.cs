using System.ComponentModel.DataAnnotations;

namespace ThreadsApp.Models
{
    public class Repost
    {
        [Key]
        public int Id { get; set; }

        [StringLength(500, ErrorMessage = "The text can't be longer than 500 characters")]
        public string? Content { get; set; }
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<PostRepost>? PostReposts { get; set; }

    }
}
