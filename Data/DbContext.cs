using CarsiDekor.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        b.Entity<Category>().Property(c => c.Name).HasMaxLength(100);
        b.Entity<Project>().Property(p => p.Title).HasMaxLength(200);
        b.Entity<ContactMessage>().Property(m => m.FullName).HasMaxLength(100);
    }
}