using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;
using System.ComponentModel.DataAnnotations;

namespace SualtiRoketSistemi.Pages.Gorevler
{
    [Authorize(Policy = "AdminOnly")]
    public class GorevDuzenleModel : PageModel
    {
        private readonly RoketContext _db;
        public GorevDuzenleModel(RoketContext db) => _db = db;

        public List<Araclar> Araclar { get; set; } = new();
        public List<Kullanici> Kullanicilar { get; set; } = new();

        [BindProperty]
        public GorevForm Form { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var gorev = await _db.Gorevler.AsNoTracking().FirstOrDefaultAsync(g => g.GorevId == id);
            if (gorev == null)
            {
                TempData["Hata"] = "Düzenlenecek görev bulunamadı.";
                return RedirectToPage("/Gorevler/Index");
            }

            Form = new GorevForm
            {
                GorevId = gorev.GorevId,
                AracId = gorev.AracFkId,
                KullaniciId = gorev.KullaniciFkId,
                GorevZamani = gorev.GorevZamani,
                Durum = gorev.Durum,
                Aciklama = gorev.Aciklama
            };

            await SayfaVerileriniYukleAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await SayfaVerileriniYukleAsync();

            if (!ModelState.IsValid)
            {
                TempData["Hata"] = "Lütfen formdaki eksik veya hatalı alanları düzeltin.";
                return Page();
            }

            if (!await _db.Araclar.AnyAsync(a => a.AracId == Form.AracId))
            {
                ModelState.AddModelError(string.Empty, "Lütfen geçerli bir araç seçin.");
                return Page();
            }

            if (!await _db.Kullanicilar.AnyAsync(k => k.KayitId == Form.KullaniciId))
            {
                ModelState.AddModelError(string.Empty, "Lütfen geçerli bir sorumlu kullanıcı seçin.");
                return Page();
            }

            var gorev = await _db.Gorevler.FirstOrDefaultAsync(g => g.GorevId == Form.GorevId);
            if (gorev == null)
            {
                TempData["Hata"] = "Düzenlenecek görev bulunamadı.";
                return RedirectToPage("/Gorevler/Index");
            }

            gorev.AracFkId = Form.AracId;
            gorev.KullaniciFkId = Form.KullaniciId;
            gorev.GorevZamani = Form.GorevZamani == default ? DateTime.Now : Form.GorevZamani;
            gorev.Durum = DurumTemizle(Form.Durum);
            gorev.Aciklama = AciklamaTemizle(Form.Aciklama);

            await _db.SaveChangesAsync();
            TempData["Mesaj"] = $"✓ Görev #{gorev.GorevId} güncellendi.";
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

        public class GorevForm
        {
            public int GorevId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Araç seçilmelidir.")]
            public int AracId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Sorumlu kullanıcı seçilmelidir.")]
            public int KullaniciId { get; set; }

            [Required(ErrorMessage = "Görev zamanı zorunludur.")]
            public DateTime GorevZamani { get; set; }

            [Required(ErrorMessage = "Durum zorunludur.")]
            public string Durum { get; set; } = "Devam Ediyor";

            [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
            public string? Aciklama { get; set; }
        }
    }
}
