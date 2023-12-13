using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThreadsApp.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }

        public string Status { get; set; }
        public DateTime Date { get; set; }

 
        public string? FollowerId { get; set; }
        public string? FollowingId { get; set; }

        public virtual ApplicationUser? Follower { get; set; }
        public virtual ApplicationUser? Following { get; set; }
    }
}
