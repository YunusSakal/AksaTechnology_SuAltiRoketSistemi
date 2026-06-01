using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SualtiRoketSistemi.Models;
using SualtiRoketSistemi.Services;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SualtiRoketSistemi.Pages.Telemetri
{
    public class IndexModel : PageModel
    {
        private readonly ImuDosyaService _imuSvc;
        private const int SayfaBasinaKayit = 100;

        public IndexModel(ImuDosyaService imuSvc) => _imuSvc = imuSvc;

        public List<string> DosyaListesi { get; set; } = new();
        public string? SeciliDosya { get; set; }
        public List<ImuVeri> ImuVeriler { get; set; } = new();
        public string ImuJsonData { get; set; } = "[]";
        public int Sayfa { get; set; } = 1;
        public int ToplamSayfa { get; set; } = 1;
        public int ToplamSatir { get; set; }

        public void OnGet(string? dosya, int sayfa = 1)
        {
            DosyaListesi = _imuSvc.GetDosyaListesi();
            SeciliDosya = dosya ?? DosyaListesi.FirstOrDefault();

            if (!string.IsNullOrEmpty(SeciliDosya))
            {
                var tumVeriler = _imuSvc.ParseDosya(SeciliDosya);
                ToplamSatir = tumVeriler.Count;
                ToplamSayfa = Math.Max(1, (int)Math.Ceiling(tumVeriler.Count / (double)SayfaBasinaKayit));
                Sayfa = Math.Clamp(sayfa, 1, ToplamSayfa);

                ImuVeriler = tumVeriler.Skip((Sayfa - 1) * SayfaBasinaKayit).Take(SayfaBasinaKayit).ToList();

                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                ImuJsonData = JsonSerializer.Serialize(tumVeriler.Select(v => new
                {
                    t = v.Timestamp,
                    timeS = v.TimeSeconds,
                    sampleNo = v.SampleNo,
                    phase = v.Phase,
                    state = v.State,
                    ax = v.Ax,
                    ay = v.Ay,
                    az = v.Az,
                    gx = v.Gx,
                    gy = v.Gy,
                    gz = v.Gz,
                    roll = v.RollDeg,
                    pitch = v.PitchDeg,
                    yaw = v.YawDeg,
                    depth = v.DepthM,
                    distanceFromStart = v.DistanceFromStartM,
                    lateralError = v.LateralErrorM,
                    distanceFromShore = v.DistanceFromShoreM,
                    motor = v.MotorStatus,
                    leak = v.LeakStatus,
                    evt = v.Event,
                    success = v.SuccessFlag,
                    safeZoneOk = v.SafeZoneOk,
                    pitchAngleOk = v.PitchAngleOk,
                    surfaceOk = v.SurfaceOk,
                    launchSignal = v.LaunchSignal
                }), jsonOptions);
            }
        }

        public async Task<IActionResult> OnPostUploadAsync(IFormFile? ImuDosya)
        {
            if (ImuDosya == null || ImuDosya.Length == 0)
            {
                TempData["Hata"] = "Dosya seçilmedi.";
                return RedirectToPage();
            }
            if (!ImuDosya.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Hata"] = "Yalnızca .txt dosyaları kabul edilir.";
                return RedirectToPage();
            }

            var ad = await _imuSvc.DosyaKaydet(ImuDosya);
            TempData["Mesaj"] = $"✓ '{ad}' başarıyla yüklendi.";
            return RedirectToPage(new { dosya = ad });
        }
    }
}
