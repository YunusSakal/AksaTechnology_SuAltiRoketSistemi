using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;
using SualtiRoketSistemi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Dashboard");
    options.Conventions.AuthorizeFolder("/Telemetri");
    options.Conventions.AuthorizeFolder("/Gorevler");
    options.Conventions.AuthorizeFolder("/Kullanicilar", "AdminOnly");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Giris";
        options.LogoutPath = "/Account/Cikis";
        options.AccessDeniedPath = "/Account/Yetkisiz";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SRS.Auth";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=App_Data/srs.db";
var dbPath = connectionString.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase).Trim();
var dbDirectory = Path.GetDirectoryName(Path.GetFullPath(dbPath));
if (!string.IsNullOrWhiteSpace(dbDirectory)) Directory.CreateDirectory(dbDirectory);

builder.Services.AddDbContext<RoketContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ImuDosyaService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoketContext>();
    db.Database.EnsureCreated();
    await EnsureSchemaAsync(db);
    await SeedDataAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/imu/{dosyaAdi}", (string dosyaAdi, ImuDosyaService svc) =>
{
    var veriler = svc.ParseDosya(dosyaAdi);
    return Results.Json(veriler);
}).RequireAuthorization();

app.Run();

static async Task SeedDataAsync(RoketContext db)
{
    if (!await db.Yetkiler.AnyAsync())
    {
        db.Yetkiler.AddRange(new Yetki { YetkiAdi = "Admin" }, new Yetki { YetkiAdi = "Kullanici" });
        await db.SaveChangesAsync();
    }

    if (!await db.Araclar.AnyAsync())
    {
        db.Araclar.AddRange(
            new Araclar { AracAdi = "SRS-Alpha", DonanimSurumu = "v1.4.2" },
            new Araclar { AracAdi = "SRS-Beta", DonanimSurumu = "v1.5.0" }
        );
        await db.SaveChangesAsync();
    }

    var adminRol = await db.Yetkiler.SingleAsync(y => y.YetkiAdi == "Admin");
    var kullaniciRol = await db.Yetkiler.SingleAsync(y => y.YetkiAdi == "Kullanici");

    if (!await db.Kullanicilar.AnyAsync(k => k.Email == "admin@roket.gov.tr"))
    {
        var admin = new Kullanici
        {
            KullaniciAdi = "Sistem",
            KullaniciSoyadi = "Yöneticisi",
            KullaniciId = "USR-ADMIN-0001",
            Email = "admin@roket.gov.tr",
            SifreHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
        };
        db.Kullanicilar.Add(admin);
        await db.SaveChangesAsync();
        db.KullaniciYetkileri.Add(new KullaniciYetkileri { KullaniciFkId = admin.KayitId, YetkiFkId = adminRol.YetkiId });
        await db.SaveChangesAsync();
    }

    if (!await db.Kullanicilar.AnyAsync(k => k.Email == "user@roket.gov.tr"))
    {
        var user = new Kullanici
        {
            KullaniciAdi = "Test",
            KullaniciSoyadi = "Kullanıcı",
            KullaniciId = "USR-TEAM-0001",
            Email = "user@roket.gov.tr",
            SifreHash = BCrypt.Net.BCrypt.HashPassword("Kullanici@123")
        };
        db.Kullanicilar.Add(user);
        await db.SaveChangesAsync();
        db.KullaniciYetkileri.Add(new KullaniciYetkileri { KullaniciFkId = user.KayitId, YetkiFkId = kullaniciRol.YetkiId });
        await db.SaveChangesAsync();
    }
}

static async Task EnsureSchemaAsync(RoketContext db)
{
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE Gorevler ADD COLUMN Aciklama TEXT NULL"); }
    catch { }
}
