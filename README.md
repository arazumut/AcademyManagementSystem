# Akademi Yönetim Sistemi (AYS)

Akademi Yönetim Sistemi, eğitim kurumları için geliştirilmiş kapsamlı bir yönetim çözümüdür. Bu sistem, öğrencilerin, eğitmenlerin, derslerin, sınavların ve duyuruların etkin bir şekilde yönetilmesini sağlar.

## Teknolojiler

- ASP.NET Core 7.0
- Entity Framework Core
- Identity Framework
- SQL Server
- Serilog
- FluentValidation

## Özellikler

- **Rol Tabanlı Yetkilendirme**: Admin, Eğitimci ve Öğrenci rolleri
- **Ders Yönetimi**: Dersler, kategoriler ve dönemler
- **Sınav Yönetimi**: Sınavlar ve sınav sonuçları
- **Öğrenci Kayıt Sistemi**: Derslere öğrenci kaydı
- **Duyuru Sistemi**: Kurslar için duyurular

## Veri Modeli

- Kullanıcılar (Admin, Eğitimci, Öğrenci)
- Kurslar ve Kategoriler
- Dönemler
- Sınavlar ve Sınav Sonuçları
- Duyurular

## Kurulum ve Çalıştırma

1. Projeyi klonlayın:
   ```
   git clone <repo-url>
   ```

2. Proje dizinine gidin:
   ```
   cd AYS
   ```

3. Veritabanını oluşturun:
   ```
   dotnet ef database update
   ```

4. Uygulamayı çalıştırın:
   ```
   dotnet run
   ```

5. Tarayıcınızda https://localhost:5001 adresine gidin

## Varsayılan Kullanıcı

- E-posta: admin@ays.com
- Şifre: Admin@123

## Katkıda Bulunma

1. Projeyi forklayın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın 