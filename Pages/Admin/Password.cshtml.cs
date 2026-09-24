using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CarsiDekor.Web.Data;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin;

public class PasswordModel : PageModel
{
    private readonly AppDbContext _db;

    public PasswordModel(AppDbContext db)
    {
        _db = db;
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi yazın.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifreyi yazın.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Yeni şifre en az 8 karakter olmalı.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifreyi tekrar yazın.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler aynı değil.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Forbid();
        }

        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return Forbid();
        }

        if (!PasswordService.Verify(Input.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("Input.CurrentPassword", "Mevcut şifre yanlış.");
            return Page();
        }

        user.PasswordHash = PasswordService.Hash(Input.NewPassword);
        await _db.SaveChangesAsync();

        TempData["Flash"] = "Şifreniz değiştirildi.";
        return RedirectToPage("/Admin/Index");
    }
}
