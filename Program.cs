using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Örnek kategorileri ekle (tablo boşsa)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { Name = "Yüzüklük", Slug = "yuzukluk", DisplayOrder = 1 },
            new Category { Name = "Kolye Büstü", Slug = "kolye-busu", DisplayOrder = 2 },
            new Category { Name = "Manken", Slug = "manken", DisplayOrder = 3 },
            new Category { Name = "Tablo", Slug = "tablo", DisplayOrder = 4 },
            new Category { Name = "Komple Vitrin", Slug = "komple-vitrin", DisplayOrder = 5 }
        );
        db.SaveChanges();
    }

        if (!db.Projects.Any())
    {
        var cats = db.Categories.ToDictionary(c => c.Slug);

        db.Projects.AddRange(
            new Project
            {
                Title = "Altın Yüzüklük Vitrini",
                Description = "Ceviz kaplama gövde üzerine ışıklı yüzüklük dizaynı.",
                Materials = "Ceviz kaplama, pleksi, LED aydınlatma",
                CoverImagePath = "https://placehold.co/800x600?text=Yuzukluk",
                IsFeatured = true,
                CategoryId = cats["yuzukluk"].Id
            },
            new Project
            {
                Title = "Kolye Büstü Seti",
                Description = "Kadife kaplı, üç boy kolye büstü seti.",
                Materials = "MDF, kadife kaplama",
                CoverImagePath = "https://placehold.co/800x600?text=Kolye+Busu",
                CategoryId = cats["kolye-busu"].Id
            },
            new Project
            {
                Title = "Vitrin Mankeni",
                Description = "Takı sergilemeye uygun ölçüde küçük manken.",
                Materials = "Fiber, deri kaplama",
                CoverImagePath = "https://placehold.co/800x600?text=Manken",
                CategoryId = cats["manken"].Id
            },
            new Project
            {
                Title = "Komple Mağaza Vitrini",
                Description = "Ölçüye göre hazırlanmış duvar ve cam vitrin sistemi.",
                Materials = "Lake boya, cam, LED",
                CoverImagePath = "https://placehold.co/800x600?text=Komple+Vitrin",
                IsFeatured = true,
                CategoryId = cats["komple-vitrin"].Id
            }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();