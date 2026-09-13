# Clinic Reservation

[![CI](https://github.com/AmirehsanK/Clinic-Reservation/actions/workflows/ci.yml/badge.svg)](https://github.com/AmirehsanK/Clinic-Reservation/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-enabled-425CC7)

<!-- Demo GIF goes here: record sign-in → create slots → filter records, save as docs/demo.gif -->

A clinic appointment management system built with ASP.NET Core MVC. Staff sign in
with a one-time code sent to their mobile, then manage patients, publish bookable
time slots, and keep records of who attended and what they paid.

## What it does

**Patients** — register patients individually or paste in a batch, search the
register by name, mobile, national ID, age, gender or notes, edit details, and
remove patients. A patient with appointment history cannot be deleted by
accident; removing them and their history is a separate, explicit action.

**Reservations** — publish bookable time slots one at a time, or generate a
month's worth in one pass by picking the weekdays, the times of day and a visit
duration. Overlapping slots are rejected, both against existing slots and against
others in the same submission. Slots already attached to an appointment cannot be
deleted; free ones can be removed individually or in bulk.

**Records** — every appointment links a patient to a slot and carries a status
(reserved, attended, cancelled), a payment type (cash, credit card, not paid) and
an amount. Filter by any of those, by patient, or by free text.

**Admins** — manage the staff accounts that can sign in. Mobile numbers are
unique.

Nothing is ever hard-deleted. Rows are flagged instead, and a global query filter
keeps them out of every read path.

## How it is put together

| Project | Responsibility |
| --- | --- |
| `Clinic.Data` | EF Core entities, `AppDbContext`, and a generic repository. Owns soft-delete filtering and provider selection. |
| `Clinic.Application` | Services holding the business rules, the DTOs they exchange, paging helpers and image utilities. Returns a `BaseResponse` carrying success, a message and per-item errors. |
| `Clinic.Mvc` | Controllers, Razor views (Tabler UI), cookie authentication, and development seeding. |
| `DropTestDb` | Small console helper that drops the database so a run can start clean. |
| `Clinic.Tests` | xUnit tests for the services, against in-memory SQLite. |

Sign-in is passwordless: the user submits a mobile number, the app generates a
six-digit code, and `ISmsService` delivers it. The shipped implementation writes
the code to the log so the app is usable without an SMS account — replace it with
a real gateway for production. Codes are single-use, expire after two minutes,
and a new one cannot be requested until the previous one lapses.

## Engineering notes

**No double-booking under concurrency.** Booking reads a slot, checks it is
free, then marks it reserved. Two staff members booking the same slot at the
same moment would both pass that check and both write an appointment. The
`Reserved` flag is an EF Core concurrency token, so the update carries
`WHERE Reserved = 0`; the second writer matches no row, its whole
`SaveChanges` (record insert included) rolls back, and it gets a clean
"already reserved" answer. No schema change, and it behaves the same on SQL
Server and SQLite. `BookingConcurrencyTests` reproduces the race
deterministically: it pauses one request between its read and its write while a
second one books the slot, and fails if the token is removed.

**Observability.** Traces, metrics and logs are exported over OTLP when
`OTEL_EXPORTER_OTLP_ENDPOINT` is set, including two business counters,
`clinic.bookings.created` and `clinic.bookings.conflicts`. Without the variable,
nothing is exported. `/health/live` reports the process is up and
`/health/ready` also checks the database.

## Running it with Docker

Needs only Docker.

```bash
docker compose up --build
```

| | |
| --- | --- |
| App | http://localhost:8080 — sign in with `09120000000` |
| Sign-in code | `docker compose logs app`, look for `OTP for 09120000000` |
| Telemetry dashboard | http://localhost:18888 — traces, metrics and structured logs |
| Health | http://localhost:8080/health/ready |

This starts SQL Server 2022, the app seeded with demo data, and the .NET Aspire
dashboard as an OpenTelemetry collector. The SA password in
`docker-compose.yml` is a local throwaway; override it with `MSSQL_SA_PASSWORD`.

## Running it locally

Requires the .NET 10 SDK and SQL Server (LocalDB is fine). The connection
string lives in `Clinic.Mvc/appsettings.json`:

```
Server=(localdb)\mssqllocaldb;Database=ClinicReservationDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

```bash
dotnet run --project Clinic.Mvc
```

The database is created on first start. In Development it is also seeded with an
admin (mobile `09120000000`), 20 patients, and a few weeks of slots and records,
so there is something to look at immediately. Outside Development, seeding only
runs when `Database__SeedDemoData=true` is set, as the Docker demo does.

Because there is no real SMS gateway, read the sign-in code from the application
log — look for a line like `OTP for 09120000000: 123456`.

## Running it without SQL Server

The app can run against a single-file SQLite database instead, which needs no
server and no install. This is for local testing only; SQL Server stays the
default and `appsettings.json` is untouched. Two environment variables select it:

```bash
Database__Provider=Sqlite
ConnectionStrings__DefaultConnection="Data Source=clinic-test.db"
```

There is a `Clinic.Mvc (SQLite test)` launch profile that sets both, and
`DropTestDb` reads the same variables so it can reset either database.

## Tests

Two layers, both run by [CI](.github/workflows/ci.yml) on every push and pull
request, alongside a Docker Compose smoke test that waits for `/health/ready`.

**Unit and integration tests** (`Clinic.Tests`, xUnit) run the services against
an in-memory SQLite database: the booking race, overlap rules including slots
generated in the same batch, delete protection, soft-delete filtering, and the
one-time-code rules.

```bash
dotnet test
```

**End-to-end** — `full_test.sh` drives the real HTTP surface — 105 assertions
covering every controller action, the filters, the duplicate and overlap rules,
the soft-delete protections, and the antiforgery checks. `run-tests.ps1` wires up
the whole cycle: drop the database, build, start the app, run the suite, shut
down.

```bash
pwsh ./run-tests.ps1
```

Pass `-SkipBuild` to reuse existing build output. It needs `bash` on the machine
(Git for Windows provides it; on Windows it is preferred over WSL's `bash`, which
cannot reach the app on localhost).

## Notes

- Deleting anything is a `POST` guarded by an antiforgery token. The buttons are
  real forms, so they work whether or not the page's JavaScript loaded.
- `Directory.Build.props` and `Directory.Packages.props` at the solution root are
  intentionally near-empty. They stop MSBuild inheriting settings from whatever
  happens to sit in a parent folder.
- reCAPTCHA is registered and has a config section, but no site key is set and
  the sign-in form renders no widget, so it currently validates nothing.
