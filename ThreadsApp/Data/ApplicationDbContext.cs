using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<PostRepost> PostReposts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // relatie many-to-many intre Post si Repost (PostRepost)

            // definire primary key compus
            modelBuilder.Entity<PostRepost>()
                        .HasKey(ab => new
                        {
                            ab.Id,
                            ab.PostId,
                            ab.RepostId
                        });

            // definire relatii cu modelele Post si Repost (FK)
            modelBuilder.Entity<PostRepost>()
                        .HasOne(ab => ab.Post)
                        .WithMany(ab => ab.PostReposts)
                        .HasForeignKey(ab => ab.PostId);

            modelBuilder.Entity<PostRepost>()
                        .HasOne(ab => ab.Repost)
                        .WithMany(ab => ab.PostReposts)
                        .HasForeignKey(ab => ab.RepostId);

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
            }
        }
}