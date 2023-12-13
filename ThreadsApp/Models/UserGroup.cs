using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ThreadsApp.Models
{
    public class UserGroup
    {
        [Key]
        public int Id { get; set; }

        public string MembershipStatus { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public int? GroupId {  get; set; }
        public virtual Group? Group { get; set; }
    }
}
