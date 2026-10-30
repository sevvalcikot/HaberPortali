# HaberPortalı

İnternet Programcılığı I dersi proje ödevi: **Haber Portalı** (öğrenci numarası son rakamı 4).

ASP.NET Core MVC ile hazırlanmış; ziyaretçilere açık bir haber sitesi ve editörlerin içerik yönettiği bir yönetim panelinden oluşur.

| | |
|---|---|
| Altyapı | ASP.NET Core MVC (.NET 10), Visual Studio 2022 ve üzeri |
| Veritabanı | SQL Server, Entity Framework Core, **Code-First** |
| Veri erişimi | **Repository** deseni (`IRepository<T>`, `Repository<T>`, `NewsRepository`) |
| Görsel tasarım | Bootstrap 5, Bootstrap Icons |
| Oturum | **Cookie tabanlı** giriş ve rol bazlı yetkilendirme (Yönetici, Editör, Üye) |
| AJAX | jQuery ile: haber/kategori/kullanıcı durum anahtarları, yorum onaylama ve silme, etiket ekleme ve silme |

---

## İlk çalıştırma

1. **SQL Server** kurulu olmalı. Proje varsayılan olarak `SQLEXPRESS` adlı örneğe bağlanır. Bağlantı satırı `HaberPortali/appsettings.json` dosyasındadır:

   ```
   Server=.\SQLEXPRESS;Database=HaberPortaliDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
   ```

2. `HaberPortali.sln` dosyasını Visual Studio ile açın. NuGet paketleri otomatik indirilir.

3. **Tools → NuGet Package Manager → Package Manager Console** penceresini açın ve sırayla çalıştırın:

   ```
   Add-Migration InitialCreate
   Update-Database
   ```

   İlk komut `Migrations` klasörünü oluşturur, ikincisi veritabanını ve tabloları kurar.

4. **F5** ile projeyi başlatın. İlk açılışta örnek kategoriler, haberler, kullanıcılar ve yorumlar otomatik eklenir.

### Giriş bilgileri

Yönetim paneli: `/Admin` (sitenin üst menüsündeki "Editör girişi" bağlantısı)

| Rol | E-posta | Şifre | Yetki |
|---|---|---|---|
| Yönetici | admin@haberportali.com | Admin123! | Her şey |
| Editör | editor@haberportali.com | Editor123! | Haber, yorum, etiket |
| Üye | uye@haberportali.com | Uye12345! | Panele giremez |

### Farklı bilgisayarda (ör. okul laboratuvarı)

Bilgisayarda SQL Server Express yerine **LocalDB** varsa `appsettings.json` içindeki satırı şununla değiştirin, sonra `Update-Database` çalıştırın:

```
Server=(localdb)\\MSSQLLocalDB;Database=HaberPortaliDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

---

## İş paketleri ve projedeki karşılıkları

| İş paketi | Nerede |
|---|---|
| Veritabanının tasarımı | `Models/` (User, Category, News, Comment, Tag, NewsTag), ilişkiler `Data/AppDbContext.cs` |
| Model ve ViewModel'ler | `Models/`, `ViewModels/` |
| Veritabanı bağlantısı ve Migration | `appsettings.json`, `Program.cs` (`AddDbContext`), `Migrations/` |
| Repository yapısı | `Repositories/` |
| Yönetici paneli tasarımı | `Areas/Admin/Views/Shared/_AdminLayout.cshtml`, `wwwroot/css/admin.css` |
| Cookie bazlı oturum ve yetkilendirme | `Program.cs` (`AddCookie`), `Areas/Admin/Controllers/AccountController.cs`, `[Authorize(Roles = ...)]` |
| Yönetici sayfaları | `Areas/Admin/Controllers/` ve `Areas/Admin/Views/` (gösterge paneli, haber, kategori, etiket, yorum, kullanıcı) |
| AJAX kullanımı | `wwwroot/js/admin.js`, `Areas/Admin/Views/Tag/Index.cshtml`; sunucu tarafı `Toggle`, `ToggleApprove`, `ToggleActive`, `Tag/Create`, `Tag/Delete` |

## Veritabanı

```
Users 1───* News *───1 Categories
  │          │
  │          ├──* Comments *──1 Users
  │          │
  │          └──* NewsTags *──1 Tags      (Haber ↔ Etiket çoka-çok)
```

- Kategoride haber varsa kategori silinemez (`Restrict`).
- Haber silinince yorumları ve etiket bağlantıları da silinir (`Cascade`).
- Yorumlar yönetici/editör onayından sonra sitede görünür.

## Proje yapısı

```
HaberPortali/
├── Areas/Admin/          Yönetim paneli (controller + view)
├── Controllers/          Sitenin sayfaları (ana sayfa, haber, kategori, etiket, arama)
├── Data/                 AppDbContext ve örnek veri (DbInitializer)
├── Helpers/              Slug üretimi, metin ve kullanıcı yardımcıları
├── Models/               Veritabanı tabloları (entity)
├── Repositories/         Repository deseni
├── Services/             Görsel yükleme servisi
├── ViewComponents/       Kategori menüsü, son dakika bandı
├── ViewModels/           Formlar ve sayfalar için modeller
├── Views/                Sitenin görünümleri
└── wwwroot/              CSS, JavaScript, görseller
```
