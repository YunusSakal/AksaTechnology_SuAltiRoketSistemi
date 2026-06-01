using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;
using System.Security.Claims;

namespace SualtiRoketSistemi.Pages.Account
{
    public class GirisModel : PageModel
    {
        private readonly RoketContext _db;
        public GirisModel(RoketContext db) => _db = db;

        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Sifre { get; set; } = string.Empty;
        [BindProperty] public bool BeniHatirla { get; set; }

        [BindProperty] public string KullaniciAdi { get; set; } = string.Empty;
        [BindProperty] public string KullaniciSoyadi { get; set; } = string.Empty;
        [BindProperty] public string KayitEmail { get; set; } = string.Empty;
        [BindProperty] public string KayitSifre { get; set; } = string.Empty;
        [BindProperty] public string KayitSifreTekrar { get; set; } = string.Empty;

        public string KayitAdi { get; set; } = string.Empty;
        public string KayitSoyadi { get; set; } = string.Empty;
        public string? HataMesaji { get; set; }
        public string? BasariMesaji { get; set; }
        public string AktifSekme { get; set; } = "giris";

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/Dashboard/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostGirisAsync()
        {
            var kullanici = await _db.Kullanicilar
                .Include(k => k.KullaniciYetkileri)
                .ThenInclude(ky => ky.Yetki)
                .FirstOrDefaultAsync(k => k.Email == Email.Trim().ToLower());

            if (kullanici == null || !BCrypt.Net.BCrypt.Verify(Sifre, kullanici.SifreHash))
            {
                HataMesaji = "E-posta veya şifre hatalı.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, kullanici.KayitId.ToString()),
                new(ClaimTypes.Name, $"{kullanici.KullaniciAdi} {kullanici.KullaniciSoyadi}"),
                new(ClaimTypes.Email, kullanici.Email),
            };

            var roller = kullanici.KullaniciYetkileri
                .Select(ky => ky.Yetki?.YetkiAdi)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .ToList();
            if (!roller.Any()) roller.Add("Kullanici");
            foreach (var rol in roller) claims.Add(new Claim(ClaimTypes.Role, rol!));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = BeniHatirla,
                ExpiresUtc = BeniHatirla ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
            return RedirectToPage("/Dashboard/Index");
        }

        public async Task<IActionResult> OnPostKayitAsync()
        {
            AktifSekme = "kayit";
            KayitAdi = KullaniciAdi;
            KayitSoyadi = KullaniciSoyadi;

            if (string.IsNullOrWhiteSpace(KullaniciAdi) || string.IsNullOrWhiteSpace(KullaniciSoyadi) || string.IsNullOrWhiteSpace(KayitEmail) || string.IsNullOrWhiteSpace(KayitSifre))
            {
                HataMesaji = "Lütfen tüm alanları doldurunuz.";
                return Page();
            }
            if (KayitSifre != KayitSifreTekrar)
            {
                HataMesaji = "Şifreler eşleşmiyor.";
                return Page();
            }
            if (KayitSifre.Length < 6)
            {
                HataMesaji = "Şifre en az 6 karakter olmalıdır.";
                return Page();
            }

            var temizEmail = KayitEmail.Trim().ToLower();
            if (await _db.Kullanicilar.AnyAsync(k => k.Email == temizEmail))
            {
                HataMesaji = "Bu e-posta adresi zaten kayıtlı.";
                return Page();
            }

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = KullaniciAdi.Trim(),
                KullaniciSoyadi = KullaniciSoyadi.Trim(),
                KullaniciId = GenerateSystemUserId(),
                Email = temizEmail,
                SifreHash = BCrypt.Net.BCrypt.HashPassword(KayitSifre)
            };

            _db.Kullanicilar.Add(yeniKullanici);
            await _db.SaveChangesAsync();

            var yetki = await _db.Yetkiler.FirstOrDefaultAsync(y => y.YetkiAdi == "Kullanici") ?? await _db.Yetkiler.FirstAsync();
            _db.KullaniciYetkileri.Add(new KullaniciYetkileri { KullaniciFkId = yeniKullanici.KayitId, YetkiFkId = yetki.YetkiId });
            await _db.SaveChangesAsync();

            BasariMesaji = $"Kayıt başarılı! Kullanıcı ID: {yeniKullanici.KullaniciId}. Şimdi giriş yapabilirsiniz.";
            AktifSekme = "giris";
            Email = temizEmail;
            return Page();
        }

        private static string GenerateSystemUserId()
        {
            var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"USR-{DateTime.UtcNow:yyMMdd}-{token}";
        }
    }
}
