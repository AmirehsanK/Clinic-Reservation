using Clinic.Application.Services.Implementation;
using Clinic.Data.Context;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Tests;

/// <summary>
/// An in-memory SQLite database that lives as long as this object. Every context
/// handed out shares the one open connection, so separate contexts see each
/// other's committed writes - which is what lets a test play two concurrent
/// requests against the same data.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestDatabase()
    {
        _connection.Open();
        using var context = NewContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public static IGenericRepository<T> Repo<T>(AppDbContext context) where T : BaseEntity =>
        new GenericRepository<T>(context, context.Set<T>());

    public static RecordService RecordService(AppDbContext context) =>
        new(Repo<Patient>(context), Repo<ReserveRecord>(context), Repo<Reservation>(context));

    public static ReservationService ReservationService(AppDbContext context) =>
        new(Repo<Reservation>(context), Repo<ReserveRecord>(context));

    public async Task<int> AddSlot(DateTime start, int minutes = 30, bool reserved = false)
    {
        await using var context = NewContext();
        var slot = new Reservation
        {
            ReserveTime = start,
            EndReserveTime = start.AddMinutes(minutes),
            Reserved = reserved,
            CreateDate = DateTime.Now,
            LastUpdateDate = DateTime.Now
        };
        context.Reservations.Add(slot);
        await context.SaveChangesAsync();
        return slot.Id;
    }

    public void Dispose() => _connection.Dispose();
}
