using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Projects;

public class IndexModel : PageModel
{
    private const int PageSize = 15;

    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public IndexModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    public List<Project> Projects { get; private set; } = new();
    public string? Q { get; private set; }
    public int Total { get; private set; }
    public PagerInfo Pager { get; private set; } = new(1, 1, null);

    public async Task OnGetAsync(string? q, int p = 1)
    {
        Q = q?.Trim();

        var query = _db.Projects.AsNoTracking().Include(x => x.Category).AsQueryable();
        if (!string.IsNullOrEmpty(Q))
        {
            query = query.Where(x => x.Title.Contains(Q));
        }

        // Arama, sayım ve sayfalama veritabanında yapılır; sadece o sayfanın kayıtları çekilir
        Total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
        p = Math.Clamp(p, 1, totalPages);

        Projects = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((p - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        Pager = new PagerInfo(p, totalPages, Q);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string? q, int p = 1)
    {
        var project = await _db.Projects.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);

        if (project is not null)
        {
            var files = project.Images.Select(i => (string?)i.ImagePath).Append(project.CoverImagePath).ToList();

            _db.Projects.Remove(project);   // fotoğraf kayıtları da birlikte silinir
            await _db.SaveChangesAsync();

            // Kayıt silindikten sonra dosyalar da diskten kaldırılır
            foreach (var file in files)
            {
                _images.Delete(file);
            }

            TempData["Flash"] = $"\"{project.Title}\" silindi.";
        }

        return RedirectToPage(new { q, p });
    }
}
