using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public class CategoryNode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CoverImagePath { get; set; }
        public List<CategoryNode> Children { get; set; } = new();
        public List<Project> Projects { get; set; } = new();

        // Admin > Kategoriler ekranında kategoriye doğrudan eklenen ek fotoğraflar
        public List<CategoryImage> Images { get; set; } = new();

        public bool IsLeaf => Children.Count == 0;

        // Ağaçtaki yeri: 0 = ana kategori, 1 = alt kategori, 2+ = onların altındakiler
        public CategoryNode? Parent { get; set; }
        public int Depth { get; set; }

        // Sol menüde sadece ilk 2 seviye görünür; daha derindeki bir kategori seçiliyken
        // menüde işaretlenecek satır (2. seviyedeki atası) budur.
        public int NavId => Depth <= 1 || Parent is null ? Id : Parent.NavId;

        // En üstteki atadan başlayarak tüm üst kategoriler (kendisi hariç) - yol çubuğu için
        public List<CategoryNode> Ancestors()
        {
            var list = new List<CategoryNode>();
            for (var p = Parent; p != null; p = p.Parent) list.Insert(0, p);
            return list;
        }

        // Bu kategorinin kendi galerisindeki fotoğraf sayısı (kapak + ek fotoğraflar + projeler)
        public int PhotoCount =>
            Projects.Count + Images.Count + (string.IsNullOrEmpty(CoverImagePath) ? 0 : 1);

        // Alt kategori kartlarında gösterilecek küçük görsel
        public string? Thumb =>
            !string.IsNullOrEmpty(CoverImagePath) ? CoverImagePath
            : Projects.Select(p => p.CoverImagePath).FirstOrDefault(x => !string.IsNullOrEmpty(x))
              ?? Images.FirstOrDefault()?.ImagePath;

        public bool Contains(int id) => Id == id || Children.Any(c => c.Contains(id));

        // Kendisi + tüm alt kategorileri (her seviyeden) tek liste olarak
        public IEnumerable<CategoryNode> Flatten() =>
            new[] { this }.Concat(Children.SelectMany(c => c.Flatten()));
    }

    public List<CategoryNode> Tree { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Images)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var byParent = categories.Where(c => c.ParentCategoryId != null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.DisplayOrder).ToList());

        var projectsByCategory = projects.GroupBy(p => p.CategoryId).ToDictionary(g => g.Key, g => g.ToList());

        CategoryNode Build(Category c, CategoryNode? parent, int depth)
        {
            var node = new CategoryNode
            {
                Parent = parent,
                Depth = depth,
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                CoverImagePath = c.CoverImagePath,
                Images = c.Images.OrderBy(i => i.DisplayOrder).ToList()
            };

            if (projectsByCategory.TryGetValue(c.Id, out var ps))
                node.Projects = ps;

            if (byParent.TryGetValue(c.Id, out var kids))
                node.Children = kids.Select(k => Build(k, node, depth + 1)).ToList();

            return node;
        }

        Tree = categories.Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => Build(c, null, 0))
            .ToList();
    }
}
