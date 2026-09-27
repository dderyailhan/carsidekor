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
        public List<CategoryNode> Children { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public bool IsLeaf => Children.Count == 0;
    }

    public List<CategoryNode> Tree { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var categories = await _db.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync();

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var byParent = categories.Where(c => c.ParentCategoryId != null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.DisplayOrder).ToList());

        var projectsByCategory = projects.GroupBy(p => p.CategoryId).ToDictionary(g => g.Key, g => g.ToList());

        CategoryNode Build(Category c)
        {
            var node = new CategoryNode { Id = c.Id, Name = c.Name, Slug = c.Slug };
            if (byParent.TryGetValue(c.Id, out var kids))
                node.Children = kids.Select(Build).ToList();
            if (node.Children.Count == 0 && projectsByCategory.TryGetValue(c.Id, out var ps))
                node.Projects = ps;
            return node;
        }

        Tree = categories.Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .Select(Build)
            .ToList();
    }
}
