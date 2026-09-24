using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages;

public class CategoriesModel : PageModel
{
    private readonly AppDbContext _db;

    public CategoriesModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Category> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _db.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
}