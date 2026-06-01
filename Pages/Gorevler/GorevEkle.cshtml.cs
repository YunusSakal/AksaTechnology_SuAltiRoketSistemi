using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;

namespace SualtiRoketSistemi.Pages.Gorevler
{
    [Authorize(Policy = "AdminOnly")]
    public class GorevEkleModel : PageModel
    {
        private readonly RoketContext _db;
        public GorevEkleModel(RoketContext db) => _db = db;

        public List<Araclar> Araclar { get; set; } = new();
        public List<Kullanici> Kullanicilar { get; set; } = new();

        public async Task OnGetAsync() => await SayfaVerileriniYukleAsync();

        public async Task<IActionResult> OnPostAsync(int aracId, int kullaniciId, DateTime gorevZamani, string? durum, string? aciklama)
        {
            await SayfaVerileriniYukleAsync();

            if (aracId <= 0 || !await _db.Araclar.AnyAsync(a => a.AracId == aracId))
            {
                TempData["Hata"] = "Lütfen geçerli bir araç seçin.";
                return Page();
            }

            if (kullaniciId <= 0 || !await _db.Kullanicilar.AnyAsync(k => k.KayitId == kullaniciId))
            {
                TempData["Hata"] = "Lütfen geçerli bir sorumlu kullanıcı seçin.";
                return Page();
            }

            var temizDurum = DurumTemizle(durum);
            var gorev = new SualtiRoketSistemi.Models.Gorevler
            {
                KullaniciFkId = kullaniciId,
                AracFkId = aracId,
                GorevZamani = gorevZamani == default ? DateTime.Now : gorevZamani,
                Durum = temizDurum,
                Aciklama = AciklamaTemizle(aciklama)
            };

            _db.Gorevler.Add(gorev);
            await _db.SaveChangesAsync();

            TempData["Mesaj"] = $"✓ Görev başarıyla oluşturuldu. Görev No: #{gorev.GorevId}";
            return RedirectToPage("/Gorevler/Index");
        }

        private async Task SayfaVerileriniYukleAsync()
        {
            Araclar = await _db.Araclar.OrderBy(a => a.AracAdi).ToListAsync();
            Kullanicilar = await _db.Kullanicilar.OrderBy(k => k.KullaniciAdi).ThenBy(k => k.KullaniciSoyadi).ToListAsync();
        }

        private static string DurumTemizle(string? durum)
        {
            var izinli = new[] { "Devam Ediyor", "Tamamlandi", "Iptal" };
            return izinli.Contains(durum) ? durum! : "Devam Ediyor";
        }

        private static string? AciklamaTemizle(string? aciklama)
        {
            if (string.IsNullOrWhiteSpace(aciklama)) return null;
            aciklama = aciklama.Trim();
            return aciklama.Length > 1000 ? aciklama[..1000] : aciklama;
        }
    }
}
