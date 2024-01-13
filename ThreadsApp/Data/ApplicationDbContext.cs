using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ThreadsApp.Models;

namespace ThreadsApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Repost> Reposts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // relatie many-to-many intre Post si User (Like)

            // definire primary key compus
            modelBuilder.Entity<Like>()
                        .HasKey(ab => new
                        {
                            ab.Id,
                            ab.UserId,
                            ab.PostId
                        });

            // definire relatii cu modelele Post si User (FK)
            modelBuilder.Entity<Like>()
                        .HasOne(ab => ab.Post)
                        .WithMany(ab => ab.Likes)
                        .HasForeignKey(ab => ab.PostId);

            modelBuilder.Entity<Like>()
                        .HasOne(ab => ab.User)
                        .WithMany(ab => ab.Likes)
                        .HasForeignKey(ab => ab.UserId);

            // many-to-many relationship between users 

            modelBuilder.Entity<Follow>()
                        .HasOne(f => f.Follower)
                        .WithMany(u => u.Followings)
                        .HasForeignKey(f => f.FollowerId)
                        .OnDelete(DeleteBehavior.NoAction);
           
            modelBuilder.Entity<Follow>()
                        .HasOne(f => f.Following)
                        .WithMany(u => u.Followers)
                        .HasForeignKey(f => f.FollowingId)
                        .OnDelete(DeleteBehavior.NoAction); ;
            

            // unique constraint in UserGroups entries of combination UserId, GroupId
            modelBuilder.Entity<UserGroup>()
                        .HasIndex(ug => new { ug.UserId, ug.GroupId })
                        .IsUnique();

            // when a record in the group table is deleted -> delete all related records in the UserGroups table
            modelBuilder.Entity<UserGroup>()
                        .HasOne(ug => ug.Group)
                        .WithMany(g => g.UserGroups)
                        .HasForeignKey(ug => ug.GroupId)
                        .OnDelete(DeleteBehavior.Cascade);

        }


    }
}