Clinic Reservation Project developed with the help of a course (Mostly for review purposes).

## Running the app

The app targets SQL Server. `appsettings.json` points at LocalDB by default:

```
Server=(localdb)\mssqllocaldb;Database=ClinicReservationDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

`dotnet run --project Clinic.Mvc` creates and seeds the database on first start
(seeding is Development-only).

## Testing without SQL Server

If SQL Server / LocalDB is not installed, the app can run against a single-file
SQLite database instead. This is a testing convenience only — SQL Server remains
the default and nothing in `appsettings.json` changes.

Select it with two environment variables:

```bash
Database__Provider=Sqlite
ConnectionStrings__DefaultConnection="Data Source=clinic-test.db"
```

There is also a `Clinic.Mvc (SQLite test)` launch profile that sets both.

`run-tests.ps1` wires up the whole cycle — drop the SQLite file, build, start the
app, run `full_test.sh` against it, shut down:

```bash
pwsh ./run-tests.ps1
```

Pass `-SkipBuild` to reuse existing build output. It needs `bash` (Git for
Windows) for `full_test.sh`.

`DropTestDb` reads the same two environment variables, so it can reset either
database:

```bash
dotnet run --project DropTestDb
```
