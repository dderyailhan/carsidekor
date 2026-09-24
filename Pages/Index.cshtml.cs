using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    // Slider'daki bir slayt
    public record HeroSlide(string Image, string Title, string Subtitle, string LinkUrl, string LinkText);

    // Kategori kutucuğu: ad, adres, proje sayısı ve en yeni projenin görseli
    public class CategoryTile
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProjectCount { get; set; }
        public string? Cover { get; set; }
    }

    // Gerçek fotoğrafları wwwroot/images/slides klasörüne koyunca
    // aşağıdaki adresleri "/images/slides/1.jpg" gibi değiştirmen yeterli.
    public List<HeroSlide> Slides { get; } = new()
    {
        new("https://placehold.co/1920x900/2a2a31/d4d4da?text=Fotograf+1",
            "Ürününüz vitrinde parlasın",
            "Kuyumcular için ölçüye özel vitrin tasarımı ve marangozluk.",
            "/Projects", "Projelerimizi inceleyin"),
        new("https://placehold.co/1920x900/25252b/d4d4da?text=Fotograf+2",
            "Yüzüklükten kolye büstüne",
            "Takınızı en iyi gösteren standlar, mankenler ve tablolar.",
            "/Projects", "İşlerimize göz atın"),
        new("https://placehold.co/1920x900/2d2d34/d4d4da?text=Fotograf+3",
            "Komple vitrin çözümleri",
            "Mağazanızın ölçüsüne göre tasarlanır ve üretilir.",
            "/Contact", "Teklif isteyin"),
    };

    public List<CategoryTile> Categories { get; private set; } = new();
    public List<Project> Featured { get; private set; } = new();

    public async Task OnGetAsync()
    {
        // Sayımı ve son proje görselini veritabanı hesaplıyor, uygulamaya sadece sonuç geliyor.
        Categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryTile
            {
                Name = c.Name,
                Slug = c.Slug,
                ProjectCount = c.Projects.Count(p => p.IsPublished),
                Cover = c.Projects
                    .Where(p => p.IsPublished)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => p.CoverImagePath)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // Önce "öne çıkan" işaretliler, eksik kalırsa en yeniler
        Featured = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .Take(3)
            .ToListAsync();
    }
}
