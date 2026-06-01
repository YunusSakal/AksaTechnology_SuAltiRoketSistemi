using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SualtiRoketSistemi.Models;
using SualtiRoketSistemi.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SualtiRoketSistemi.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly RoketContext _db;
        private readonly ImuDosyaService _imuSvc;

        public IndexModel(RoketContext db, ImuDosyaService imuSvc)
        {
            _db = db;
            _imuSvc = imuSvc;
        }

        public int ToplamGorev { get; set; }
        public int AktifGorev { get; set; }
        public int ToplamTelemetri { get; set; }
        public bool AktifSizinti { get; set; }
        public int ToplamKullanici { get; set; }
        public int ImuDosyaSayisi { get; set; }
        public bool AdminMi { get; set; }

        public List<SualtiRoketSistemi.Models.Gorevler> SonGorevler { get; set; } = new();
        public string ImuJsonData { get; set; } = "[]";
        public string TelemetriJsonData { get; set; } = "[]";

        public async Task OnGetAsync()
        {
            AdminMi = User.IsInRole("Admin");
            var kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var gorevQuery = _db.Gorevler.AsQueryable();
            if (!AdminMi)
                gorevQuery = gorevQuery.Where(g => g.KullaniciFkId == kullaniciId);

            ToplamGorev = await gorevQuery.CountAsync();
            AktifGorev = await gorevQuery.CountAsync(g => g.Durum == "Devam Ediyor");
            ToplamKullanici = AdminMi ? await _db.Kullanicilar.CountAsync() : 1;
            ImuDosyaSayisi = _imuSvc.GetDosyaListesi().Count;

            var gorevIdleri = await gorevQuery.Select(g => g.GorevId).ToListAsync();
            var telemetriQuery = _db.Telemetri_Kayitlari.AsQueryable();
            if (!AdminMi)
                telemetriQuery = telemetriQuery.Where(t => gorevIdleri.Contains(t.GorevFkId));

            ToplamTelemetri = await telemetriQuery.CountAsync();
            AktifSizinti = await telemetriQuery.AnyAsync(t => t.SiviSizintisi_bool);

            SonGorevler = await gorevQuery
                .OrderByDescending(g => g.GorevZamani)
                .Take(5)
                .ToListAsync();

            var dosyalar = _imuSvc.GetDosyaListesi();
            if (dosyalar.Any())
            {
                var veriler = _imuSvc.ParseDosya(dosyalar.First()).TakeLast(50).ToList();
                ImuJsonData = JsonSerializer.Serialize(veriler.Select(v => new {
                    timestamp = v.Timestamp,
                    ax = v.Ax,
                    ay = v.Ay,
                    az = v.Az,
                    gx = v.Gx,
                    gy = v.Gy,
                    gz = v.Gz
                }));
            }

            var tel = await telemetriQuery
                .OrderByDescending(t => t.ZamanDamgasi)
                .Take(30)
                .OrderBy(t => t.ZamanDamgasi)
                .ToListAsync();

            TelemetriJsonData = JsonSerializer.Serialize(tel.Select(t => new {
                zaman = t.ZamanDamgasi.ToString("HH:mm:ss"),
                derinlikZ = t.Derinlik_Z,
                pitch = t.Pitch_Acisi,
                roll = t.Roll_Acisi,
                yaw = t.Yaw_Acisi
            }));
        }
    }
}
