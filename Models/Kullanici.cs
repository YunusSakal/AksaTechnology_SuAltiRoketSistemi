namespace SualtiRoketSistemi.Models
{
    public class Kullanici
    {
        // Veritabanı ilişki anahtarı. Kullanıcıya gösterilmez.
        public int KayitId { get; set; }

        // TC yerine kullanılan, sistemin otomatik ürettiği kullanıcı kimliği.
        public string KullaniciId { get; set; } = string.Empty;

        public string KullaniciAdi { get; set; } = string.Empty;
        public string KullaniciSoyadi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SifreHash { get; set; } = string.Empty;

        public ICollection<KullaniciYetkileri> KullaniciYetkileri { get; set; } = new List<KullaniciYetkileri>();
        public ICollection<Gorevler> Gorevler { get; set; } = new List<Gorevler>();
    }
}
