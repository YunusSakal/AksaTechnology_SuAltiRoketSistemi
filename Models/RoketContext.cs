using Microsoft.EntityFrameworkCore;

namespace SualtiRoketSistemi.Models
{
    public class RoketContext : DbContext
    {
        public RoketContext(DbContextOptions<RoketContext> options) : base(options) { }

        public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
        public DbSet<Yetki> Yetkiler => Set<Yetki>();
        public DbSet<KullaniciYetkileri> KullaniciYetkileri => Set<KullaniciYetkileri>();
        public DbSet<Araclar> Araclar => Set<Araclar>();
        public DbSet<Gorevler> Gorevler => Set<Gorevler>();
        public DbSet<Telemetri_Kayitlari> Telemetri_Kayitlari => Set<Telemetri_Kayitlari>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Kullanici>(entity =>
            {
                entity.HasKey(k => k.KayitId);
                entity.HasIndex(k => k.Email).IsUnique();
                entity.HasIndex(k => k.KullaniciId).IsUnique();
                entity.Property(k => k.KullaniciId).HasMaxLength(32).IsRequired();
                entity.Property(k => k.KullaniciAdi).HasMaxLength(80).IsRequired();
                entity.Property(k => k.KullaniciSoyadi).HasMaxLength(80).IsRequired();
                entity.Property(k => k.Email).HasMaxLength(160).IsRequired();
                entity.Property(k => k.SifreHash).IsRequired();
            });

            modelBuilder.Entity<Yetki>(entity =>
            {
                entity.HasKey(y => y.YetkiId);
                entity.HasIndex(y => y.YetkiAdi).IsUnique();
                entity.Property(y => y.YetkiAdi).HasMaxLength(40).IsRequired();
            });

            modelBuilder.Entity<KullaniciYetkileri>(entity =>
            {
                entity.HasKey(ky => ky.KullaniciYetkiId);
                entity.HasIndex(ky => new { ky.KullaniciFkId, ky.YetkiFkId }).IsUnique();
                entity.HasOne(ky => ky.Kullanici)
                    .WithMany(k => k.KullaniciYetkileri)
                    .HasForeignKey(ky => ky.KullaniciFkId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ky => ky.Yetki)
                    .WithMany(y => y.KullaniciYetkileri)
                    .HasForeignKey(ky => ky.YetkiFkId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Araclar>(entity =>
            {
                entity.HasKey(a => a.AracId);
                entity.Property(a => a.AracAdi).HasMaxLength(100).IsRequired();
                entity.Property(a => a.DonanimSurumu).HasMaxLength(40).IsRequired();
            });

            modelBuilder.Entity<Gorevler>(entity =>
            {
                entity.HasKey(g => g.GorevId);
                entity.Property(g => g.Durum).HasMaxLength(40).IsRequired();
                entity.Property(g => g.Aciklama).HasMaxLength(1000);
                entity.HasOne(g => g.Kullanici)
                    .WithMany(k => k.Gorevler)
                    .HasForeignKey(g => g.KullaniciFkId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(g => g.Arac)
                    .WithMany(a => a.Gorevler)
                    .HasForeignKey(g => g.AracFkId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Telemetri_Kayitlari>(entity =>
            {
                entity.HasKey(t => t.TelemetriId);
                entity.HasOne(t => t.Gorev)
                    .WithMany(g => g.TelemetriKayitlari)
                    .HasForeignKey(t => t.GorevFkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
