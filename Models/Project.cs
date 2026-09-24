namespace CarsiDekor.Web.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Materials { get; set; }              // "Ceviz kaplama, pleksi, LED"
    public string? CoverImagePath { get; set; }
    public bool IsFeatured { get; set; }                // ana sayfa slider'ında çıksın mı
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public List<ProjectImage> Images { get; set; } = new();
}