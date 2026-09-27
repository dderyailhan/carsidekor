using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public IndexModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    public List<(Category Category, int Depth)> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var all = await _db.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync();

        var byParent = all.Where(c => c.ParentCategoryId != null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.DisplayOrder).ToList());

        void Walk(Category c, int depth)
        {
            Rows.Add((c, depth));
            if (byParent.TryGetValue(c.Id, out var kids))
                foreach (var k in kids) Walk(k, depth + 1);
        }

        foreach (var top in all.Where(c => c.ParentCategoryId == null).OrderBy(c => c.DisplayOrder))
            Walk(top, 0);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Children)
            .Include(c => c.Projects)
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return RedirectToPage();

        if (category.Children.Any())
        {
            TempData["Flash"] = $"\"{category.Name}\" silinemedi: önce alt kategorilerini silin.";
        }
        else if (category.Projects.Any())
        {
            TempData["Flash"] = $"\"{category.Name}\" silinemedi: bu kategoride hâlâ proje var.";
        }
        else
        {
            // Diskten silinecek dosyaları veritabanı kaydı silinmeden önce not al
            var filesToDelete = new List<string?> { category.CoverImagePath };
            filesToDelete.AddRange(category.Images.Select(i => i.ImagePath));

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            foreach (var path in filesToDelete)
            {
                _images.Delete(path);
            }

            TempData["Flash"] = $"\"{category.Name}\" silindi.";
        }

        return RedirectToPage();
    }
}
