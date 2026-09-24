using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Projects;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;

    public DetailsModel(AppDbContext db)
    {
        _db = db;
    }

    public Project Item { get; private set; } = null!;
    public List<Project> Related { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPublished);

        // Olmayan ya da yayında olmayan projeye 404 dön
        if (project is null)
        {
            return NotFound();
        }

        Item = project;

        // Aynı kategoriden diğer işler (en fazla 3)
        Related = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsPublished && p.CategoryId == project.CategoryId && p.Id != project.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToListAsync();

        return Page();
    }
}
