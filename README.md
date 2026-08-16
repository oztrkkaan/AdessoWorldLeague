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
