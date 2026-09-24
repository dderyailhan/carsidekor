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