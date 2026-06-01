using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;

namespace SualtiRoketSistemi.Pages.Kullanicilar
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly RoketContext _db;
        public IndexModel(RoketContext db) => _db = db;

        public List<Kullanici> Kullanicilar { get; set; } = new();
        public List<Yetki> Yetkiler { get; set; } = new();

        public async Task OnGetAsync()
        {
            Kullanicilar = await _db.Kullanicilar
                .Include(k => k.KullaniciYetkileri).ThenInclude(ky => ky.Yetki)
                .Include(k => k.Gorevler)
                .OrderBy(k => k.KullaniciAdi).ThenBy(k => k.KullaniciSoyadi)
                .ToListAsync();
            Yetkiler = await _db.Yetkiler.ToListAsync();
        }

        public async Task<IActionResult> OnPostEkleAsync(string ad, string soyad, string email, string sifre, int yetkiId)
        {
            email = email.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
            {
                TempData["Hata"] = "Lütfen tüm alanları doldurun.";
                return RedirectToPage();
            }
            if (await _db.Kullanicilar.AnyAsync(k => k.Email == email))
            {
                TempData["Hata"] = "Bu e-posta adresi zaten kayıtlı.";
                return RedirectToPage();
            }
            if (!await _db.Yetkiler.AnyAsync(y => y.YetkiId == yetkiId))
            {
                TempData["Hata"] = "Geçerli bir rol seçin.";
                return RedirectToPage();
            }

            var kullanici = new Kullanici
            {
                KullaniciAdi = ad.Trim(),
                KullaniciSoyadi = soyad.Trim(),
                Email = email,
                KullaniciId = GenerateSystemUserId(),
                SifreHash = BCrypt.Net.BCrypt.HashPassword(sifre)
            };
            _db.Kullanicilar.Add(kullanici);
            await _db.SaveChangesAsync();

            _db.KullaniciYetkileri.Add(new KullaniciYetkileri { KullaniciFkId = kullanici.KayitId, YetkiFkId = yetkiId });
            await _db.SaveChangesAsync();

            TempData["Mesaj"] = $"✓ Kullanıcı eklendi. Kullanıcı ID: {kullanici.KullaniciId}";
            return RedirectToPage();
        }

        private static string GenerateSystemUserId()
        {
            var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"USR-{DateTime.UtcNow:yyMMdd}-{token}";
        }

        public async Task<IActionResult> OnPostSilAsync(int kayitId)
        {
            var mevcutId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (kayitId == mevcutId)
            {
                TempData["Hata"] = "Kendinizi silemezsiniz.";
                return RedirectToPage();
            }

            var k = await _db.Kullanicilar.Include(x => x.Gorevler).FirstOrDefaultAsync(x => x.KayitId == kayitId);
            if (k == null)
            {
                TempData["Hata"] = "Kullanıcı bulunamadı.";
                return RedirectToPage();
            }
            if (k.Gorevler.Any())
            {
                TempData["Hata"] = "Bu kullanıcıya bağlı görevler var. Önce görevleri başka kullanıcıya aktarın veya silin.";
                return RedirectToPage();
            }

            _db.Kullanicilar.Remove(k);
            await _db.SaveChangesAsync();
            TempData["Mesaj"] = "✓ Kullanıcı silindi.";
            return RedirectToPage();
        }
    }
}
