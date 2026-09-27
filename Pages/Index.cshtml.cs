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

    // Kategori kutucuğu: ad, adres, proje sayısı ve gösterilecek görsel
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
            "/Projects", "Kategorilerimizi inceleyin"),
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
        var allCategories = await _db.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.ParentCategoryId, c.Name, c.Slug, c.DisplayOrder, c.CoverImagePath })
            .ToListAsync();

        var publishedProjects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .Select(p => new { p.CategoryId, p.CoverImagePath, p.CreatedAt })
            .ToListAsync();

        var byParent = allCategories.Where(c => c.ParentCategoryId != null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Bir kategorinin kendisi + tüm alt/alt-alt kategorilerinin id'lerini toplar
        HashSet<int> DescendantIds(int rootId)
        {
            var result = new HashSet<int> { rootId };
            void Walk(int id)
            {
                if (!byParent.TryGetValue(id, out var kids)) return;
                foreach (var k in kids)
                {
                    if (result.Add(k.Id)) Walk(k.Id);
                }
            }
            Walk(rootId);
            return result;
        }

        // Ana sayfada sadece en üst düzey kategoriler kart olarak gösterilir
        // (Vitrinler, Bankolar, Nişler, Diğerleri gibi) — alt tipler tek tek listelenmez.
        Categories = allCategories
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .Select(top =>
            {
                var ids = DescendantIds(top.Id);
                var projectsInBranch = publishedProjects.Where(p => ids.Contains(p.CategoryId)).ToList();

                return new CategoryTile
                {
                    Name = top.Name,
                    Slug = top.Slug,
                    ProjectCount = projectsInBranch.Count,
                    // Önce admin panelde bu kategori için yüklenen kapak fotoğrafı kullanılır;
                    // yoksa altındaki en yeni projenin kapağı yedek olarak gösterilir.
                    Cover = top.CoverImagePath ?? projectsInBranch
                        .Where(p => !string.IsNullOrEmpty(p.CoverImagePath))
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.CoverImagePath)
                        .FirstOrDefault()
                };
            })
            .ToList();

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
