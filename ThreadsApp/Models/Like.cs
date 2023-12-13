using System.ComponentModel.DataAnnotations;

namespace ThreadsApp.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public int? PostId { get; set; }
        public string? UserId { get; set; }
        public virtual Post? Post { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
