using System.ComponentModel.DataAnnotations;
namespace SualtiRoketSistemi.Models
{
    public class KullaniciYetkileri
    {
        [Key]
        public int KullaniciYetkiId { get; set; }
        public int KullaniciFkId { get; set; }
        public int YetkiFkId { get; set; }

        public Kullanici? Kullanici { get; set; }
        public Yetki? Yetki { get; set; }
    }
}
