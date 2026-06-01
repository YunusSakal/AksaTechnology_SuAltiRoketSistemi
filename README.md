# SARA (Sualtı Roket Aracı) Yönetim Paneli

SARA Yönetim Paneli, Sualtı Roket Aracı'nın operasyonel süreçlerini, kullanıcı yetkilendirmelerini, telemetri veri akışını ve sistem konfigürasyonlarını merkezi bir web arayüzünden izlemek ve yönetmek amacıyla geliştirilmiş tam kapsamlı bir yönetim sistemidir. 

Proje; hafif, taşınabilir ve hızlı kurulum avantajı sağlaması adına **ASP.NET Core MVC** mimarisi ve **SQLite** veritabanı altyapısı ile geliştirilmiştir. Ekstra bir SQL Server veya SQLEXPRESS kurulumu gerektirmeden doğrudan çalışır.

---

## 🚀 Genel Tanıtım ve Sistem Mimarisi

Yönetim paneli, sualtı roket sistemlerinin test ve operasyon aşamalarında ihtiyaç duyulan tüm idari ve teknik fonksiyonları tek bir çatı altında toplar. Sistem temel olarak iki ana rolden oluşmaktadır:

* **Yönetici (Admin) Paneli:** Sistem ayarları, kullanıcı yetkilendirmeleri, veri tabanı bakımı, ham veri izleme ve log mekanizmalarının yönetildiği en üst düzey kontrol merkezidir.
* **Operatör (Kullanıcı) Paneli:** Sahadaki teknik personelin ve operatörlerin yarışma/test verilerini yüklediği, telemetri çıktılarını analiz ettiği operasyonel arayüzdür.


## 🛠️ Kurulum ve Çalıştırma

Projeyi kendi bilgisayarınızda çalıştırmak için terminalde aşağıdaki komutları sırasıyla giriniz:

```bash
cd SualtiRoketSistemi
dotnet restore
dotnet run
```


 Tarayıcıda terminal ekranında yazan adrese gidin (Genelde https://localhost:xxxx veya http://localhost:xxxx formatındadır). İlk çalıştırmada veritabanı tabloları otomatik olarak oluşturulacaktır.
## 👤 Varsayılan Hesaplar

### Admin (Yönetici) Hesabı
* **E-posta:** admin@roket.gov.tr
* **Şifre:** `Admin@123`

### Operatör (Kullanıcı) Hesabı
* **E-posta:** user@roket.gov.tr
* **Şifre:** `Kullanici@123`

## ✨ Öne Çıkan Özellikler

### 🔐 Kullanıcı Kimlik Yönetimi
* Kullanıcı kimliği `KullaniciId` yapısı ile yönetilirken, veritabanı ilişki anahtarı `KayitId` olarak izole edilmiştir.
* Kullanıcılara gösterilen ID, sistem tarafından otomatik olarak `USR-...` formatında üretilir.
* Kişisel verilerin korunması amacıyla kayıt ekranlarında TC kimlik numarası istenmez.
* Kullanıcı yönetimi ekranında şeffaf bir **Kullanıcı ID** kolonu yer alır.



### 📊 TXT Veri Entegrasyonu
* Virgülle ayrılmış standart telemetri formatı (`timestamp, ax, ay, az, gx, gy, gz`) tam olarak desteklenir.
* Sekmeli yarışma veri formatları sistem tarafından otomatik tanınır: `time_s`, `accel_x_mps2`, `gyro_x_dps`, `roll_deg`, `pitch_deg`, `yaw_deg`, `depth_m`, `distance_from_shore_m`, `lateral_error_m`, `event`.
* Sistemin test edilebilmesi için projeye hazır örnek veri dosyaları entegre edilmiştir:
  * `wwwroot/imu_data/ klasöründen ulaşılabilir.



## Not

Temiz kurulum için eski `App_Data/srs.db` dosyası varsa silip projeyi yeniden çalıştırmanız önerilir. Bu zip içinde temiz kaynak kodu verildiği için ilk çalıştırmada veritabanı otomatik oluşur.
