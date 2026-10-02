using BlogApi.Constants;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Post> Posts { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<PostLike> PostLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(x => x.Role)
            .HasMaxLength(20)
            .HasDefaultValue(Roles.User);

        modelBuilder.Entity<Post>()
            .HasMany(x => x.Categories)
            .WithMany(x => x.Posts)
            .UsingEntity(j => j.ToTable("PostCategories"));

        modelBuilder.Entity<PostLike>()
            .HasKey(x => new
            {
                x.UserId,
                x.PostId
            });

        modelBuilder.Entity<PostLike>()
            .HasOne(x => x.User)
            .WithMany(x => x.PostLikes)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<PostLike>()
            .HasOne(x => x.Post)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.PostId);
    }
}