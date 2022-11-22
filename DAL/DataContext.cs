using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(f => f.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
               .HasIndex(f => f.Name)
               .IsUnique();
            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostReactions)
                .WithOne(r => r.ReactionPost);
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.CommentReactions)
                .WithOne(c => c.ReactionComment);
            modelBuilder.Entity<PostReaction>()
                .HasIndex(f => new { f.ReactionPostId, f.ReactionAuthorId })
                .IsUnique();
            modelBuilder.Entity<CommentReaction>()
                .HasIndex(f => new { f.ReactionCommentId, f.ReactionAuthorId })
                .IsUnique();

            //Table-Per-Type
            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
            modelBuilder.Entity<PostPhoto>().ToTable(nameof(PostPhotos));
            modelBuilder.Entity<CommentReaction>().ToTable(nameof(CommentReactions));
            modelBuilder.Entity<PostReaction>().ToTable(nameof(PostReactions));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder) => builder.UseNpgsql(b => b.MigrationsAssembly("API"));

        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> Sessions => Set<UserSession>();
        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<PostPhoto> PostPhotos => Set<PostPhoto>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostReaction> PostReactions => Set<PostReaction>();
        public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
    }
}
