using System.ComponentModel.DataAnnotations;
using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Categories;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    public EditModel(AppDbContext db) => _db = db;

    public class InputModel
    {
        [Required(ErrorMessage = "Kategori adını yazın.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug yazın.")]
        [StringLength(450)]
        public string Slug { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<SelectListItem> ParentOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadParentOptionsAsync(id);

        if (id is null) return Page();

        var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        Input = new InputModel
        {
            Name = category.Name,
            Slug = category.Slug,
            DisplayOrder = category.DisplayOrder,
            ParentCategoryId = category.ParentCategoryId
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        await LoadParentOptionsAsync(id);

        Category? category = null;
        if (id is not null)
        {
            category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return NotFound();
        }

        var slug = Input.Slug.Trim().ToLowerInvariant();
        if (await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id))
        {
            ModelState.AddModelError("Input.Slug", "Bu slug başka bir kategoride kullanılıyor.");
        }

        if (id is not null && Input.ParentCategoryId is not null)
        {
            var descendantIds = await GetDescendantIdsAsync(id.Value);
            if (Input.ParentCategoryId == id || descendantIds.Contains(Input.ParentCategoryId.Value))
            {
                ModelState.AddModelError("Input.ParentCategoryId", "Bir kategori kendi altına veya kendi alt kategorilerinden birinin altına taşınamaz.");
            }
        }

        if (!ModelState.IsValid) return Page();

        if (category is null)
        {
            category = new Category();
            _db.Categories.Add(category);
        }

        category.Name = Input.Name.Trim();
        category.Slug = slug;
        category.DisplayOrder = Input.DisplayOrder;
        category.ParentCategoryId = Input.ParentCategoryId;

        await _db.SaveChangesAsync();

        TempData["Flash"] = $"\"{category.Name}\" kaydedildi.";
        return RedirectToPage("/Admin/Categories/Index");
    }

    private async Task<HashSet<int>> GetDescendantIdsAsync(int rootId)
    {
        var all = await _db.Categories.AsNoTracking()
            .Select(c => new { c.Id, c.ParentCategoryId }).ToListAsync();

        var result = new HashSet<int>();
        void Walk(int parentId)
        {
            foreach (var c in all.Where(x => x.ParentCategoryId == parentId))
            {
                result.Add(c.Id);
                Walk(c.Id);
            }
        }
        Walk(rootId);
        return result;
    }

    private async Task LoadParentOptionsAsync(int? excludeId)
    {
        var all = await _db.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync();

        var exclude = excludeId is null ? new HashSet<int>() : await GetDescendantIdsAsync(excludeId.Value);
        if (excludeId is not null) exclude.Add(excludeId.Value);

        var options = new List<SelectListItem> { new("(Üst kategori yok — ana kategori)", "") };

        var byParent = all.Where(c => c.ParentCategoryId != null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.DisplayOrder).ToList());

        void Walk(Category c, string prefix)
        {
            if (!exclude.Contains(c.Id))
                options.Add(new SelectListItem($"{prefix}{c.Name}", c.Id.ToString()));

            if (byParent.TryGetValue(c.Id, out var kids))
                foreach (var k in kids) Walk(k, prefix + "— ");
        }

        foreach (var top in all.Where(c => c.ParentCategoryId == null).OrderBy(c => c.DisplayOrder))
            Walk(top, "");

        ParentOptions = options;
    }
}
