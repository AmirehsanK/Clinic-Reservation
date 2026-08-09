using Clinic.Data.Context;
using Microsoft.EntityFrameworkCore;

// Mirrors the MVC app's provider selection so a test run can be reset regardless
// of which database it used. Override with the same environment variables the app
// reads: Database__Provider and ConnectionStrings__DefaultConnection.
var provider = Environment.GetEnvironmentVariable("Database__Provider") ?? DatabaseProvider.SqlServer;
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=ClinicReservationDb;Trusted_Connection=True;MultipleActiveResultSets=true";

var options = DatabaseProvider
    .Configure(new DbContextOptionsBuilder<AppDbContext>(), provider, connectionString)
    .Options;

await using var context = new AppDbContext(options);
var deleted = await context.Database.EnsureDeletedAsync();
Console.WriteLine(deleted
    ? $"Test database dropped ({provider})."
    : $"No test database found ({provider}).");
