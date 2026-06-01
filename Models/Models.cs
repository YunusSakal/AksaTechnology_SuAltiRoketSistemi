using System.ComponentModel.DataAnnotations;

namespace SualtiRoketSistemi.Models
{
    public class Araclar
    {
        [Key]
        public int AracId { get; set; }
        public string AracAdi { get; set; } = string.Empty;
        public string DonanimSurumu { get; set; } = string.Empty;

        public ICollection<Gorevler> Gorevler { get; set; } = new List<Gorevler>();
    }

    public class Gorevler
    {
        [Key]
        public int GorevId { get; set; }
        public int KullaniciFkId { get; set; }
        public int AracFkId { get; set; }
        public DateTime GorevZamani { get; set; }
        public string Durum { get; set; } = string.Empty;
        public string? Aciklama { get; set; }

        public Kullanici? Kullanici { get; set; }
        public Araclar? Arac { get; set; }
        public ICollection<Telemetri_Kayitlari> TelemetriKayitlari { get; set; } = new List<Telemetri_Kayitlari>();
    }

    public class Telemetri_Kayitlari
    {
        [Key]
        public int TelemetriId { get; set; }
        public int GorevFkId { get; set; }
        public DateTime ZamanDamgasi { get; set; }
        public float Derinlik_Z { get; set; }
        public float Pitch_Acisi { get; set; }
        public float Roll_Acisi { get; set; }
        public float Yaw_Acisi { get; set; }
        public bool SiviSizintisi_bool { get; set; }

        public Gorevler? Gorev { get; set; }
    }

    public class ImuVeri
    {
        public int Timestamp { get; set; }
        public double TimeSeconds { get; set; }
        public int SampleNo { get; set; }
        public string Phase { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public float Ax { get; set; }
        public float Ay { get; set; }
        public float Az { get; set; }
        public float Gx { get; set; }
        public float Gy { get; set; }
        public float Gz { get; set; }
        public float? RollDeg { get; set; }
        public float? PitchDeg { get; set; }
        public float? YawDeg { get; set; }
        public float? DepthM { get; set; }
        public float? DistanceFromStartM { get; set; }
        public float? LateralErrorM { get; set; }
        public float? DistanceFromShoreM { get; set; }
        public bool? MotorStatus { get; set; }
        public string LeakStatus { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public bool? SuccessFlag { get; set; }
        public bool? SafeZoneOk { get; set; }
        public bool? PitchAngleOk { get; set; }
        public bool? SurfaceOk { get; set; }
        public bool? LaunchSignal { get; set; }
    }
}
