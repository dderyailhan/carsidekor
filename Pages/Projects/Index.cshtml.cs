using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Category> Categories { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public string? Secili { get; set; }

    public async Task OnGetAsync(string? kategori)
    {
        Secili = kategori;

        Categories = await _db.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var query = _db.Projects
            .Include(p => p.Category)
            .Where(p => p.IsPublished);

        if (!string.IsNullOrEmpty(kategori))
        {
            query = query.Where(p => p.Category.Slug == kategori);
        }

        Projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}