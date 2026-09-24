using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CarsiDekor.Web.Data;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin;

public class LoginModel : PageModel
{
    // Kullanıcı bulunamasa bile hash kontrolü yapılır ki cevap süresinden "böyle bir kullanıcı var" anlaşılmasın
    private static readonly string DummyHash = PasswordService.Hash("kullanici-yok-sahte-sifre");

    private readonly AppDbContext _db;

    public LoginModel(AppDbContext db)
    {
        _db = db;
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Kullanıcı adını yazın.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifreyi yazın.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Admin/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var username = Input.Username.Trim();
        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username);

        var passwordOk = PasswordService.Verify(Input.Password, user?.PasswordHash ?? DummyHash);
        if (user is null || !passwordOk)
        {
            await Task.Delay(600);   // deneme-yanılmayı yavaşlatır
            ErrorMessage = "Kullanıcı adı veya şifre hatalı.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = Input.RememberMe });

        // Sadece kendi sitemizdeki adreslere yönlendir (başka siteye yönlendirme saldırısını engeller)
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Admin/Index");
    }
}
