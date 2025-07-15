
# SigortamNet Customer API

Bu proje, SigortamNet için geliştirilmiş basit bir müşteri yönetim sistemi API'sidir. 
.NET 8 Web API ile yazılmıştır ve SQLite veritabanı kullanır. API, müşteri kayıtlarını
ekleme, güncelleme, listeleme, arama ve silme (soft delete) işlemlerini destekler.

## 🚀 Kullanılan Teknolojiler

- ASP.NET Core 8 Web API
- Entity Framework Core (DB-First)
- SQLite
- Swagger (Swashbuckle)
- KPS Web Servisi (SOAP isteği ile TCKN doğrulama)

---

## 🔧 Kurulum Adımları

### 1. Proje Oluşturma

```bash
dotnet new webapi -n Backend
```

### 2. Gerekli Paketlerin Kurulumu

```bash
dotnet add Backend package Microsoft.EntityFrameworkCore.Sqlite
dotnet add Backend package Microsoft.EntityFrameworkCore.Design
dotnet add Backend package Swashbuckle.AspNetCore
```

### 3. Veritabanı Oluşturma

Öncelikle `create_customers.sql` dosyasını kullanarak veritabanını oluştur:

```bash
sqlite3 sigortamnet.db ".read create_customers.sql"
```

### 4. EF Core Scaffold (DB First)

```bash
dotnet ef dbcontext scaffold "Data Source=sigortamnet.db" Microsoft.EntityFrameworkCore.Sqlite \
--output-dir Models --context-dir Data --context SigortamNetDbContext --force --project Backend
```

### 5. `appsettings.json` Ayarı

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=sigortamnet.db"
}
```

---

## 🧩 Program.cs Yapılandırması

- Swagger aktif edildi.
- EF Core için `DbContext` DI olarak eklendi.
- Middleware pipeline oluşturuldu.

---

## 🧠 Controller Özeti

### `POST /api/customer`
Yeni müşteri ekler veya TCKN'ye göre günceller.  
**Validasyon yapılır**: TCKN, Ad, Soyad, Doğum Tarihi  
**KPS Web Servisi** ile doğrulama yapılır.

### `GET /api/customer/{id}`
ID'ye göre müşteri getirir. (Sadece `IsActive == 1` olanlar)

### `GET /api/customer/search`
Ad, soyad veya TCKN’ye göre arama yapar.  
Dönen sonuçlar maskeleme içerir (KVKK'ya uygun)

### `DELETE /api/customer/{id}`
Soft delete (müşteri silinmez, `IsActive = 0` yapılır)

---

## 📦 DTO'lar

- `CustomerRequestDto`: API'ye veri göndermek için.
- `CustomerMaskedDto`: Maskelenmiş veri döner (Ad, Soyad, TCKN, Doğum Tarihi)

---

## 🧪 Swagger

Proje çalıştırıldığında `https://localhost:{port}/swagger` üzerinden test edilebilir.

---

## 💡 Geliştirme Önerileri

- `Service` katmanı eklenerek business logic controller'dan ayrıştırılabilir.
- Unit test yazılabilir.
- Exception middleware eklenebilir.

---
