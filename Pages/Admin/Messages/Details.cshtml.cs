using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Messages;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;

    public DetailsModel(AppDbContext db)
    {
        _db = db;
    }

    public ContactMessage Item { get; private set; } = null!;
    public string? WhatsAppLink { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var message = await _db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (message is null)
        {
            return NotFound();
        }

        // Mesaja bakılınca okundu olarak işaretlenir
        if (!message.IsRead)
        {
            message.IsRead = true;
            await _db.SaveChangesAsync();
        }

        Item = message;
        WhatsAppLink = BuildWhatsAppLink(message.Phone);
        return Page();
    }

    public async Task<IActionResult> OnPostUnreadAsync(int id)
    {
        await _db.ContactMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, false));

        return RedirectToPage("/Admin/Messages/Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _db.ContactMessages.Where(m => m.Id == id).ExecuteDeleteAsync();

        TempData["Flash"] = "Mesaj silindi.";
        return RedirectToPage("/Admin/Messages/Index");
    }

    // 0532 123 45 67, 532 123 45 67 veya +90 532 ... gibi yazılan numaraları wa.me formatına çevirir
    private static string? BuildWhatsAppLink(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());

        string? number = digits switch
        {
            { Length: 12 } when digits.StartsWith("90") => digits,
            { Length: 11 } when digits.StartsWith('0') => "9" + digits,
            { Length: 10 } => "90" + digits,
            _ => null
        };

        return number is null ? null : $"https://wa.me/{number}";
    }
}
