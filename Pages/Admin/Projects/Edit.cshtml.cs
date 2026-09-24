using System.ComponentModel.DataAnnotations;
using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Projects;

// Hem "yeni proje" hem "proje düzenle" sayfası. Adreste id varsa düzenleme, yoksa yeni kayıt.
public class EditModel : PageModel
{
    private const int MaxExtraFiles = 10;

    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public EditModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Proje adını yazın.")]
        [StringLength(200, ErrorMessage = "Proje adı en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Bir kategori seçin.")]
        public int CategoryId { get; set; }

        [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
        public string? Description { get; set; }

        [StringLength(300, ErrorMessage = "Malzeme bilgisi en fazla 300 karakter olabilir.")]
        public string? Materials { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? CoverFile { get; set; }
    [BindProperty] public List<IFormFile> ExtraFiles { get; set; } = new();
    [BindProperty] public bool RemoveCover { get; set; }
    [BindProperty] public List<int> RemoveImageIds { get; set; } = new();

    public int? ProjectId { get; private set; }
    public string? CurrentCover { get; private set; }
    public List<ProjectImage> ExistingImages { get; private set; } = new();
    public List<SelectListItem> CategoryOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadCategoriesAsync();

        if (id is null)
        {
            return Page();   // yeni proje: boş form
        }

        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Title = project.Title,
            CategoryId = project.CategoryId,
            Description = project.Description,
            Materials = project.Materials,
            IsFeatured = project.IsFeatured,
            IsPublished = project.IsPublished
        };
        ShowExisting(project);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        await LoadCategoriesAsync();

        Project? project = null;
        if (id is not null)
        {
            project = await _db.Projects.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (project is null)
            {
                return NotFound();
            }
        }

        // --- Doğrulama ---
        if (Input.CategoryId > 0 && !await _db.Categories.AnyAsync(c => c.Id == Input.CategoryId))
        {
            ModelState.AddModelError("Input.CategoryId", "Seçtiğiniz kategori bulunamadı.");
        }

        var extraFiles = ExtraFiles.Where(f => f.Length > 0).ToList();
        if (extraFiles.Count > MaxExtraFiles)
        {
            ModelState.AddModelError(nameof(ExtraFiles), $"Tek seferde en fazla {MaxExtraFiles} fotoğraf yükleyebilirsiniz.");
        }

        if (CoverFile is { Length: > 0 })
        {
            var error = await _images.ValidateAsync(CoverFile);
            if (error is not null) ModelState.AddModelError(nameof(CoverFile), error);
        }

        foreach (var file in extraFiles)
        {
            var error = await _images.ValidateAsync(file);
            if (error is not null) ModelState.AddModelError(nameof(ExtraFiles), error);
        }

        if (!ModelState.IsValid)
        {
            ShowExisting(project);
            return Page();
        }

        // --- Kaydetme ---
        if (project is null)
        {
            project = new Project();
            _db.Projects.Add(project);
        }

        project.Title = Input.Title.Trim();
        project.CategoryId = Input.CategoryId;
        project.Description = Clean(Input.Description);
        project.Materials = Clean(Input.Materials);
        project.IsFeatured = Input.IsFeatured;
        project.IsPublished = Input.IsPublished;

        // Diskten silinecek eski dosyalar, veritabanı kaydı başarılı olunca silinir
        var filesToDelete = new List<string?>();

        if (CoverFile is { Length: > 0 })
        {
            filesToDelete.Add(project.CoverImagePath);
            project.CoverImagePath = await _images.SaveAsync(CoverFile);
        }
        else if (RemoveCover)
        {
            filesToDelete.Add(project.CoverImagePath);
            project.CoverImagePath = null;
        }

        // Sadece bu projeye ait fotoğraflar silinebilir
        var toRemove = project.Images.Where(i => RemoveImageIds.Contains(i.Id)).ToList();
        foreach (var image in toRemove)
        {
            filesToDelete.Add(image.ImagePath);
        }
        _db.ProjectImages.RemoveRange(toRemove);

        var nextOrder = project.Images.Except(toRemove).Select(i => i.DisplayOrder).DefaultIfEmpty(0).Max() + 1;
        foreach (var file in extraFiles)
        {
            project.Images.Add(new ProjectImage
            {
                ImagePath = await _images.SaveAsync(file),
                DisplayOrder = nextOrder++
            });
        }

        await _db.SaveChangesAsync();

        foreach (var path in filesToDelete)
        {
            _images.Delete(path);
        }

        TempData["Flash"] = $"\"{project.Title}\" kaydedildi.";
        return RedirectToPage("/Admin/Projects/Index");
    }

    private void ShowExisting(Project? project)
    {
        if (project is null) return;

        ProjectId = project.Id;
        CurrentCover = project.CoverImagePath;
        ExistingImages = project.Images.OrderBy(i => i.DisplayOrder).ToList();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        CategoryOptions = categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
