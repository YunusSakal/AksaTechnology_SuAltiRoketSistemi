namespace SualtiRoketSistemi.Models
{
    public class Yetki
    {
        public int YetkiId { get; set; }
        public string YetkiAdi { get; set; } = string.Empty; // "Admin" veya "Kullanici"

        public ICollection<KullaniciYetkileri> KullaniciYetkileri { get; set; } = new List<KullaniciYetkileri>();
    }
}
