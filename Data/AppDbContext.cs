using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options
    ) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Post> Posts { get; set; }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>()
            .HasMany(x => x.Categories)
            .WithMany(x => x.Posts)
            .UsingEntity(j => j.ToTable("PostCategories"));
    }
}

