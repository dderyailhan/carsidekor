using CarsiDekor.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryImage> CategoryImages => Set<CategoryImage>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        b.Entity<Category>().Property(c => c.Name).HasMaxLength(100);
        b.Entity<Project>().Property(p => p.Title).HasMaxLength(200);
        b.Entity<ContactMessage>().Property(m => m.FullName).HasMaxLength(100);

        // Kategori kendi kendine referans verebilir (üst/alt kategori)
        b.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kategori galeri fotoğrafları: kategori silinince onlar da silinsin
        b.Entity<CategoryImage>()
            .HasOne(ci => ci.Category)
            .WithMany(c => c.Images)
            .HasForeignKey(ci => ci.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Aynı kullanıcı adıyla iki yönetici olamaz
        b.Entity<AdminUser>().HasIndex(u => u.Username).IsUnique();
        b.Entity<AdminUser>().Property(u => u.Username).HasMaxLength(50);
        b.Entity<AdminUser>().Property(u => u.PasswordHash).HasMaxLength(200);
    }
}
