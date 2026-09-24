using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Messages;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<ContactMessage> Messages { get; private set; } = new();
    public PagerInfo Pager { get; private set; } = new(1, 1, null);
    public int Total { get; private set; }

    public async Task OnGetAsync(int p = 1)
    {
        Total = await _db.ContactMessages.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
        p = Math.Clamp(p, 1, totalPages);

        // Okunmamışlar üstte, kendi içinde en yeni en üstte
        Messages = await _db.ContactMessages
            .AsNoTracking()
            .OrderBy(m => m.IsRead)
            .ThenByDescending(m => m.CreatedAt)
            .Skip((p - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        Pager = new PagerInfo(p, totalPages, null);
    }
}
