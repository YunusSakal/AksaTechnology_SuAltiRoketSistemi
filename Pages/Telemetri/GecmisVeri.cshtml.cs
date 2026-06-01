using Microsoft.AspNetCore.Mvc.RazorPages;
using SualtiRoketSistemi.Models;
using SualtiRoketSistemi.Services;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SualtiRoketSistemi.Pages.Telemetri
{
    public class GecmisVeriModel : PageModel
    {
        private readonly ImuDosyaService _svc;
        public GecmisVeriModel(ImuDosyaService svc) => _svc = svc;

        public List<string> DosyaListesi { get; set; } = new();
        public string? Dosya1 { get; set; }
        public string? Dosya2 { get; set; }
        public List<ImuVeri> Veri1 { get; set; } = new();
        public List<ImuVeri> Veri2 { get; set; } = new();
        public string Veri1Json { get; set; } = "[]";
        public string Veri2Json { get; set; } = "[]";

        private static object Project(ImuVeri v) => new
        {
            timeS = v.TimeSeconds,
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
            lateralError = v.LateralErrorM,
            distanceFromStart = v.DistanceFromStartM,
            distanceFromShore = v.DistanceFromShoreM
        };

        public void OnGet(string? d1, string? d2)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            DosyaListesi = _svc.GetDosyaListesi();
            Dosya1 = d1 ?? DosyaListesi.FirstOrDefault();
            Dosya2 = d2;

            if (!string.IsNullOrEmpty(Dosya1))
            {
                Veri1 = _svc.ParseDosya(Dosya1);
                Veri1Json = JsonSerializer.Serialize(Veri1.Select(Project), jsonOptions);
            }
            if (!string.IsNullOrEmpty(Dosya2))
            {
                Veri2 = _svc.ParseDosya(Dosya2);
                Veri2Json = JsonSerializer.Serialize(Veri2.Select(Project), jsonOptions);
            }
        }
    }
}
