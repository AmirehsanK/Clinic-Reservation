using Microsoft.EntityFrameworkCore;

namespace Clinic.Data.Context;

/// <summary>
/// Chooses the EF Core provider for <see cref="AppDbContext"/>.
///
/// SQL Server is the default and the only provider intended for real use. SQLite
/// is a testing escape hatch for machines where SQL Server / LocalDB is not
/// available: it needs no server, no service and no install, and stores the whole
/// database in a single file that can be deleted to start clean.
///
/// Select it with the configuration key <c>Database:Provider</c> (environment
/// variable <c>Database__Provider=Sqlite</c>) plus a matching
/// <c>ConnectionStrings:DefaultConnection</c> such as
/// <c>Data Source=clinic-test.db</c>. Nothing about the SQL Server path changes
/// when the key is absent.
/// </summary>
public static class DatabaseProvider
{
    public const string SqlServer = "SqlServer";
    public const string Sqlite = "Sqlite";

    public static bool IsSqlite(string providerName) =>
        string.Equals(providerName, Sqlite, StringComparison.OrdinalIgnoreCase);

    public static DbContextOptionsBuilder Configure(
        DbContextOptionsBuilder options,
        string providerName,
        string connectionString)
    {
        return IsSqlite(providerName)
            ? options.UseSqlite(connectionString)
            : options.UseSqlServer(connectionString);
    }

    public static DbContextOptionsBuilder<TContext> Configure<TContext>(
        DbContextOptionsBuilder<TContext> options,
        string providerName,
        string connectionString)
        where TContext : DbContext
    {
        return IsSqlite(providerName)
            ? options.UseSqlite(connectionString)
            : options.UseSqlServer(connectionString);
    }
}
