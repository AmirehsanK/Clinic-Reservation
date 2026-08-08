using Clinic.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReserveRecord> ReserveRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft-deleted rows (IsDeleted = true) must never surface through normal
        // queries. A global filter keeps every repository/service query correct
        // without requiring an explicit "!IsDeleted" clause everywhere.
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Reservation>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<ReserveRecord>().HasQueryFilter(r => !r.IsDeleted);
    }
}