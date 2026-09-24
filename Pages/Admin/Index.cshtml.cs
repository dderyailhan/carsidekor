using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public int ProjectCount { get; private set; }
    public int PublishedCount { get; private set; }
    public int UnreadCount { get; private set; }
    public List<ContactMessage> LatestMessages { get; private set; } = new();

    public async Task OnGetAsync()
    {
        // Sayıları veritabanı hesaplıyor, kayıtların kendisi çekilmiyor
        ProjectCount = await _db.Projects.CountAsync();
        PublishedCount = await _db.Projects.CountAsync(p => p.IsPublished);
        UnreadCount = await _db.ContactMessages.CountAsync(m => !m.IsRead);

        LatestMessages = await _db.ContactMessages
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .ToListAsync();
    }
}
