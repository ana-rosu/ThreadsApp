using System.ComponentModel.DataAnnotations;

namespace ThreadsApp.Models
{
    public class PostRepost
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public int? PostId { get; set; }
        public int? RepostId { get; set; }

        public virtual Post? Post { get; set; }
        public virtual Repost? Repost { get; set; }

    }
}
