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
}