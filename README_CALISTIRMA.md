# Sualtı Roket Sistemi - Güncel Son Sürüm

Bu sürüm SQLite ile çalışır. SQL Server/SQLEXPRESS kurulumu gerekmez.

## Çalıştırma

```bash
cd SualtiRoketSistemi
dotnet restore
dotnet run
```

Tarayıcıda konsolda yazan adrese gidin. Genelde `https://localhost:xxxx` veya `http://localhost:xxxx` olur.

## Varsayılan hesaplar

Admin hesabı:
- E-posta: `admin@roket.gov.tr`
- Şifre: `Admin@123`

Normal kullanıcı hesabı:
- E-posta: `user@roket.gov.tr`
- Şifre: `Kullanici@123`

## Bu sürümde yapılanlar

### Kullanıcı ID dönüşümü
- Kullanıcı kimliği alanı artık `KullaniciId` olarak kullanılır.
- Veritabanı ilişki anahtarı `KayitId` olarak ayrıldı.
- Kullanıcıya gösterilen ID sistem tarafından otomatik `USR-...` formatında üretilir.
- Kayıt ve kullanıcı ekleme ekranlarında kişisel kimlik numarası istenmez.
- Kullanıcı yönetimi ekranında `Kullanıcı ID` kolonu gösterilir.

### Hareketli IMU rota paneli
- Telemetri sayfasında Aşama-1 ve Aşama-2 için hareketli su altı roketi animasyonu eklendi.
- `Oynat`, `Duraklat`, `Başa Al`, zaman kaydırıcı ve hız seçimi bulunur.
- TXT dosyasındaki 2 Hz veriler zaman sırasına göre oynatılır.
- Aşama-1 paneli; başlangıç/bitiş çizgisi, 10 m çizgisi, 50 m uzaklaşma çizgisi ve U dönüş rotasını gösterir.
- Aşama-2 paneli; 30 m güvenli atış bölgesi ve yan görünüm/yüzeye çıkış animasyonunu gösterir.
- Rota sapması, x ekseni/roll bozulması, kendi etrafında dönme/spin ve pitch yetersizliği görsel olarak işaretlenir.

### TXT veri desteği
- Eski format desteklenir: `timestamp,ax,ay,az,gx,gy,gz`
- Yeni sekmeli yarışma veri formatları desteklenir: `time_s`, `accel_x_mps2`, `gyro_x_dps`, `roll_deg`, `pitch_deg`, `yaw_deg`, `depth_m`, `distance_from_shore_m`, `lateral_error_m`, `event` vb.
- Verilen Aşama-1 ve Aşama-2 örnek dosyaları projeye eklendi:
  - `wwwroot/imu_data/asama1_basarili_rota_imu_2hz.txt`
  - `wwwroot/imu_data/asama2_basarili_rota_imu_2hz.txt`

## Not

Temiz kurulum için eski `App_Data/srs.db` dosyası varsa silip projeyi yeniden çalıştırmanız önerilir. Bu zip içinde temiz kaynak kodu verildiği için ilk çalıştırmada veritabanı otomatik oluşur.
