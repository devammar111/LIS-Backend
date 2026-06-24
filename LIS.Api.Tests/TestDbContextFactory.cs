using LIS.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LIS.Api.Tests;

/// <summary>
/// Creates a real <see cref="LisDbContext"/> backed by SQLite in-memory. The connection is kept
/// open for the lifetime of the context so the schema (created via EnsureCreated) survives.
/// SQLite exercises real relational behaviour and EF SQL translation, unlike the InMemory provider.
/// </summary>
public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public LisDbContext Create(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<LisDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptors)
            .Options;

        var context = new LisDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose() => _connection.Dispose();
}
