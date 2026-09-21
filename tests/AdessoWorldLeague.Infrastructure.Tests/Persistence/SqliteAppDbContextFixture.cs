using AdessoWorldLeague.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Tests.Persistence;

/// <summary>
/// Gerçek <see cref="AppDbContext"/>'i ilişkisel bir sağlayıcı (SQLite in-memory) üzerinde
/// çalıştırır. Böylece <c>IEntityTypeConfiguration</c> yapılandırmaları, seed verisi,
/// yabancı anahtarlar ve benzersizlik kısıtları gerçekten doğrulanabilir.
/// </summary>
public sealed class SqliteAppDbContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteAppDbContextFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
