using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;
using System.Security.Claims;

namespace SualtiRoketSistemi.Pages.Gorevler
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly RoketContext _db;
        public IndexModel(RoketContext db) => _db = db;

        public List<SualtiRoketSistemi.Models.Gorevler> Gorevler { get; set; } = new();
        public string? DurumFiltre { get; set; }
        public bool AdminMi { get; set; }

        public async Task OnGetAsync(string? durum)
        {
            await SayfaVerileriniYukleAsync(durum);
        }

        public async Task<IActionResult> OnPostSilAsync(int gorevId)
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["Hata"] = "Görev silme işlemi yalnızca admin kullanıcılar tarafından yapılabilir.";
                return RedirectToPage();
            }

            var gorev = await _db.Gorevler.FindAsync(gorevId);
            if (gorev == null)
            {
                TempData["Hata"] = "Silinecek görev bulunamadı.";
                return RedirectToPage();
            }

            _db.Gorevler.Remove(gorev);
            await _db.SaveChangesAsync();
            TempData["Mesaj"] = "✓ Görev silindi.";
            return RedirectToPage();
        }

        private async Task SayfaVerileriniYukleAsync(string? durum)
        {
            AdminMi = User.IsInRole("Admin");
            DurumFiltre = durum;

            var q = _db.Gorevler
                .Include(g => g.Arac)
                .Include(g => g.Kullanici)
                .Include(g => g.TelemetriKayitlari)
                .AsQueryable();

            if (!AdminMi)
            {
                var kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                q = q.Where(g => g.KullaniciFkId == kullaniciId);
            }

            if (!string.IsNullOrWhiteSpace(durum))
                q = q.Where(g => g.Durum == durum);

            Gorevler = await q.OrderByDescending(g => g.GorevZamani).ToListAsync();
        }
    }
}
