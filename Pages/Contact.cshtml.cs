using System.ComponentModel.DataAnnotations;
using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarsiDekor.Web.Pages;

public class ContactModel : PageModel
{
    private readonly AppDbContext _db;

    public ContactModel(AppDbContext db)
    {
        _db = db;
    }

    // Formdan gelen alanlar ve doğrulama kuralları
    public class InputModel
    {
        [Required(ErrorMessage = "Adınızı ve soyadınızı yazın.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Size ulaşabilmemiz için telefon numaranızı yazın.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası yazın.")]
        [StringLength(30, ErrorMessage = "Telefon numarası çok uzun.")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi yazın.")]
        [StringLength(150, ErrorMessage = "E-posta adresi çok uzun.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mesajınızı yazın.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mesajınız 10 ile 2000 karakter arasında olmalı.")]
        public string Message { get; set; } = string.Empty;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Botların doldurduğu, insanların görmediği gizli alan
    [BindProperty]
    public string? Website { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Gizli alan doluysa bot demektir: kaydetmeden başarılıymış gibi davran
        if (!string.IsNullOrEmpty(Website))
        {
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _db.ContactMessages.Add(new ContactMessage
        {
            FullName = Input.FullName.Trim(),
            Phone = Input.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
            Message = Input.Message.Trim()
        });
        await _db.SaveChangesAsync();

        SuccessMessage = "Mesajınız bize ulaştı. En kısa sürede size dönüş yapacağız.";
        return RedirectToPage();
    }
}
