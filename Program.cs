using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ImageStorage>();

// Giriş sistemi: başarılı girişte tarayıcıya şifreli bir çerez verilir
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.Cookie.Name = "CarsiDekor.Admin";
        options.Cookie.HttpOnly = true;                       // JavaScript çereze erişemez
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest                // geliştirirken http'de de çalışsın
            : CookieSecurePolicy.Always;                      // yayında sadece https
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    // /Admin altındaki tüm sayfalar giriş ister, sadece giriş sayfası herkese açık
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
});

var app = builder.Build();

// Başlangıç verileri
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

    // İlk yönetici hesabı. Bilgiler kodda değil, user-secrets'ta durur.
    if (!db.AdminUsers.Any())
    {
        var username = app.Configuration["Admin:Username"];
        var password = app.Configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            app.Logger.LogWarning(
                "Yönetici hesabı yok. Oluşturmak için Admin:Username ve Admin:Password ayarlarını girin.");
        }
        else
        {
            db.AdminUsers.Add(new AdminUser
            {
                Username = username.Trim(),
                PasswordHash = PasswordService.Hash(password)
            });
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Yönetim panelinden sonradan yüklenen fotoğrafları sunmak için gerekli
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // "bu kişi kim?"
app.UseAuthorization();    // "bu sayfaya girmeye yetkisi var mı?"

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
