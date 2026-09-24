# Çarşı Dekor

Kuyumcular için vitrin tasarımı ve üretimi yapan Çarşı Dekor atölyesinin tanıtım sitesi.
Yapılan işler kategoriye göre listelenir, müşteriler iletişim formu veya WhatsApp ile ulaşır.

## Özellikler
- Otomatik dönen slider'lı ana sayfa
- Kategori filtreli proje galerisi ve proje detay sayfası
- İletişim formu (doğrulamalı, mesajlar veritabanına kaydedilir)
- Mobil uyumlu tasarım, WhatsApp butonu

## Teknolojiler
- ASP.NET Core Razor Pages (.NET 10)
- Entity Framework Core 10 (Code First, migration)
- SQL Server 2022 (Docker)
- Bootstrap 5

## Kurulum
1. .NET 10 SDK ve Docker Desktop kurulu olmalı.
2. SQL Server'ı başlat:
```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<guclu-sifre>" -p 1433:1433 --name carsidekor-sql -d mcr.microsoft.com/mssql/server:2022-latest
```
3. Bağlantı bilgisini kaydet (şifre repoda tutulmaz, user-secrets kullanılır):
```bash
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=CarsiDekor;User Id=sa;Password=<guclu-sifre>;TrustServerCertificate=True"
```
4. Veritabanını oluştur:
```bash
   dotnet ef database update
```
5. Çalıştır:
```bash
   dotnet run
```
   Tarayıcıda `http://localhost:5286` adresini aç. İlk açılışta örnek kategoriler ve projeler otomatik eklenir.

## Yapılandırma
Telefon, WhatsApp, adres gibi bilgiler `Data/SiteInfo.cs` dosyasında tutulur.

## Yapılacaklar
- Yönetim paneli (proje ve fotoğraf yönetimi, gelen mesajlar)
- Gerçek proje fotoğrafları
