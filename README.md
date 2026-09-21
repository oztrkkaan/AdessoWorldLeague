# Adesso World League

Dünya Ligi kura çekimi simülasyonu yapan bir .NET 10 Web API projesidir.

## MakeDraw Ne Yapar?

`MakeDraw`, 32 takımı belirtilen grup sayısına (4 veya 8) rastgele dağıtan bir kura çekimi algoritmasıdır.

**Kurallar:**
- Toplam takım sayısı tam olarak 32 olmalıdır.
- Grup sayısı 4 veya 8 olabilir.
- Aynı gruba aynı ülkeden iki takım atanamaz.
- Takımlar rastgele sıralanarak gruplara eşit şekilde dağıtılır.

## Mimari (Clean Architecture)

| Katman | Sorumluluk |
|--------|-----------|
| **Domain** | Entity'ler, iş kuralları ve domain sabitleri (`Draw`, `Team`, `DrawGroup`, `DrawConstants`) |
| **Application** | CQRS komutları/handler'ları, validasyon kuralları (MediatR + FluentValidation) |
| **Infrastructure** | EF Core DbContext, entity konfigürasyonları, migration'lar |
| **Api** | Controller'lar, middleware'ler, DI konfigürasyonu, Swagger |

### Katman Bağımlılık Diyagramı

```
┌──────────────────────────────────────────┐
│                  Api                     │
│        (Presentation Layer)              │
└──────────┬───────────────┬───────────────┘
           │               │
           ▼               ▼
┌─────────────────┐   ┌──────────────────┐
│   Application   │   │  Infrastructure  │
│  (Use Cases)    │   │  (EF Core, DB)   │
└────────┬────────┘   └────────┬─────────┘
         │                     │
         │                     │
         ▼                     │
┌─────────────────┐            │
│     Domain      │◄───────────┘
│ (Entities/Rules)│
└─────────────────┘
```

**Bağımlılık Kuralı:** Oklar bağımlılık yönünü gösterir. İç katmanlar dış katmanları bilmez.
- `Api` → `Application`, `Infrastructure`
- `Infrastructure` → `Application` → `Domain`
- `Domain` hiçbir katmana bağımlı değildir.

## Testler

Tüm testler `tests/` klasöründe, katman başına ayrı projeler halinde konumlanmıştır.

```bash
dotnet test AdessoWorldLeague.slnx
```

**Sonuç: 195 test, tamamı başarılı (0 başarısız, 0 atlanan).**

| Test Projesi | Test Sayısı | Kapsam |
|--------------|:-----------:|--------|
| `AdessoWorldLeague.Domain.Tests` | 65 | Kura algoritması (`Draw.Make`), entity iş kuralları, domain sabitleri |
| `AdessoWorldLeague.Application.Tests` | 49 | `MakeDrawCommandHandler`, FluentValidation kuralları, `ValidationBehavior` |
| `AdessoWorldLeague.Infrastructure.Tests` | 45 | EF Core konfigürasyonları, seed verisi, FK/cascade/unique kısıtları |
| `AdessoWorldLeague.Architecture.Tests` | 36 | Katman bağımlılık yönü, framework yalıtımı, isimlendirme kuralları |
| **Toplam** | **195** | |

### Doğrulanan Başlıca Kurallar

- **Ülke kısıtı:** Hiçbir grupta aynı ülkeden iki takım bulunmaz (4 ve 8 grup için ayrı ayrı).
- **Dağıtım:** 32 takımın tamamı birer kez atanır ve gruplara eşit dağılır.
- **Çekiliş sırası:** Takımlar sırayla A → B → C… gruplarına çekilir, son gruptan sonra başa dönülür.
- **Rastgelelik:** Ardışık çekilişler farklı dağılımlar üretir.
- **Kura hiçbir zaman çıkmaza girmez:** Her iki grup sayısı için 500'er çekiliş hatasız tamamlanır.
- **Mimari:** `Domain` yalnızca BCL'e bağımlıdır; EF Core, MediatR, FluentValidation veya ASP.NET referansı içermez. `Application` somut `DbContext` yerine `IApplicationDbContext` soyutlamasını kullanır.

### Test Altyapısı

- **xUnit** — test çatısı
- **NSubstitute** — `ValidationBehavior` testlerinde `IValidator` sahteleme
- **EF Core InMemory** — Application katmanı testleri (Infrastructure'a bağımlılık olmadan)
- **EF Core SQLite (in-memory)** — Infrastructure testleri; gerçek `AppDbContext` ilişkisel sağlayıcı üzerinde çalıştığından cascade delete ve unique index gibi kısıtlar fiilen doğrulanır
- **NetArchTest.Rules** — mimari bağımlılık kuralları

## Projeyi Ayağa Kaldırma

### Gereksinimler
- .NET 10 SDK
- SQL Server (veya LocalDB)

### Adımlar

```bash
# 1. Bağımlılıkları yükle
dotnet restore

# 2. appsettings.json içindeki ConnectionString'i düzenle (Api projesi)

# 3. Migration oluştur
dotnet ef migrations add InitialCreate --project AdessoWorldLeague.Infrastructure --startup-project AdessoWorldLeague.Api

# 4. Veritabanını güncelle
dotnet ef database update --project AdessoWorldLeague.Infrastructure --startup-project AdessoWorldLeague.Api

# 5. Uygulamayı çalıştır
dotnet run --project AdessoWorldLeague.Api
```

Uygulama ayağa kalktığında Swagger UI ana sayfada (`/`) erişilebilir olacaktır.

## `.github` ve `docs` Klasörleri

Bu klasörler AI destekli geliştirme deneyimini iyileştirmek amacıyla eklenmiştir:

- **`.github/copilot-instructions.md`** — GitHub Copilot'a projeye özel yönlendirmeler verir (kullanılacak dil sürümü, mimari kurallar, kod stili vb.).
- **`docs/`** — Mimari kararlar ve kuralları dokümante eder. AI araçları bu dosyaları bağlam olarak kullanarak daha tutarlı ve projeye uygun kod üretir.

Bu dosyalar runtime'da bir işlev görmez; yalnızca AI asistanların projeyi daha iyi anlaması için referans noktası sağlar.
