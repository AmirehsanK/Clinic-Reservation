using Clinic.Data.Context;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ClinicReservationDb;Trusted_Connection=True;MultipleActiveResultSets=true")
    .Options;

await using var context = new AppDbContext(options);
var deleted = await context.Database.EnsureDeletedAsync();
Console.WriteLine(deleted ? "Test database dropped." : "No test database found.");
