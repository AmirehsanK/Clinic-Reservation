using Clinic.Data.Context;
using Clinic.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Mvc;

/// <summary>
/// Development-only convenience seeding so the app is usable and demonstrable
/// immediately after cloning, without requiring manual database access.
/// Never runs outside the Development environment.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext context, IConfiguration configuration, ILogger logger)
    {
        SeedDefaultAdmin(context, configuration, logger);
        SeedPatients(context, logger);
        SeedReservationsAndRecords(context, logger);
    }

    private static void SeedDefaultAdmin(AppDbContext context, IConfiguration configuration, ILogger logger)
    {
        if (context.Users.Any())
        {
            return;
        }

        var seedMobile = configuration["SeedAdmin:Mobile"] ?? "09120000000";
        var seedFullName = configuration["SeedAdmin:FullName"] ?? "Default Admin";

        context.Users.Add(new User
        {
            FullName = seedFullName,
            Mobile = seedMobile,
            CreateDate = DateTime.Now,
            LastUpdateDate = DateTime.Now
        });
        context.SaveChanges();

        logger.LogInformation("Seeded default admin '{Name}' with mobile {Mobile} for local sign-in.", seedFullName, seedMobile);
    }

    private static readonly (string FullName, string Mobile, string NationalId, int Age, Gender Gender, string? Description)[] SamplePatients =
    [
        ("Michael Johnson", "09121110001", "1000000001", 34, Gender.Male, "Regular checkup patient."),
        ("Sarah Williams", "09121110002", "1000000002", 28, Gender.Female, null),
        ("David Brown", "09121110003", "1000000003", 45, Gender.Male, "Follow-up for chronic condition."),
        ("Emily Davis", "09121110004", "1000000004", 31, Gender.Female, null),
        ("James Miller", "09121110005", "1000000005", 52, Gender.Male, "Prefers morning appointments."),
        ("Jessica Wilson", "09121110006", "1000000006", 24, Gender.Female, null),
        ("Robert Moore", "09121110007", "1000000007", 61, Gender.Male, "Allergic to penicillin."),
        ("Ashley Taylor", "09121110008", "1000000008", 19, Gender.Female, null),
        ("William Anderson", "09121110009", "1000000009", 39, Gender.Male, null),
        ("Amanda Thomas", "09121110010", "1000000010", 47, Gender.Female, "Referred by Dr. Lee."),
        ("Christopher Jackson", "09121110011", "1000000011", 55, Gender.Male, null),
        ("Melissa White", "09121110012", "1000000012", 29, Gender.Female, null),
        ("Daniel Harris", "09121110013", "1000000013", 41, Gender.Male, "Needs wheelchair access."),
        ("Laura Martin", "09121110014", "1000000014", 33, Gender.Female, null),
        ("Matthew Thompson", "09121110015", "1000000015", 26, Gender.Male, null),
        ("Rachel Garcia", "09121110016", "1000000016", 38, Gender.Female, "Follow-up scan required."),
        ("Andrew Martinez", "09121110017", "1000000017", 63, Gender.Male, null),
        ("Nicole Robinson", "09121110018", "1000000018", 22, Gender.Female, null),
        ("Kevin Clark", "09121110019", "1000000019", 30, Gender.NotSpecified, null),
        ("Samantha Lewis", "09121110020", "1000000020", 44, Gender.NotSpecified, "New patient intake pending."),
    ];

    private static void SeedPatients(AppDbContext context, ILogger logger)
    {
        if (context.Patients.Any())
        {
            return;
        }

        var now = DateTime.Now;
        var patients = SamplePatients.Select(p => new Patient
        {
            FullName = p.FullName,
            Mobile = p.Mobile,
            NationalId = p.NationalId,
            Age = p.Age,
            Gender = p.Gender,
            Description = p.Description,
            CreateDate = now,
            LastUpdateDate = now
        }).ToList();

        context.Patients.AddRange(patients);
        context.SaveChanges();

        logger.LogInformation("Seeded {Count} sample patients.", patients.Count);
    }

    private static readonly TimeSpan[] DailySlots = [new(9, 0, 0), new(11, 0, 0), new(14, 0, 0), new(16, 0, 0)];

    private static void SeedReservationsAndRecords(AppDbContext context, ILogger logger)
    {
        if (context.Reservations.Any())
        {
            return;
        }

        var patients = context.Patients.OrderBy(p => p.Id).ToList();
        if (patients.Count == 0)
        {
            return;
        }

        var now = DateTime.Now;
        var reservations = new List<Reservation>();

        // Past two weeks: fully booked slots with a mix of attended / cancelled records.
        for (var dayOffset = -14; dayOffset < 0; dayOffset++)
        {
            var date = now.Date.AddDays(dayOffset);
            if (date.DayOfWeek is DayOfWeek.Friday)
            {
                continue;
            }

            foreach (var slot in DailySlots)
            {
                reservations.Add(new Reservation
                {
                    ReserveTime = date.Add(slot),
                    EndReserveTime = date.Add(slot).AddMinutes(30),
                    Reserved = true,
                    CreateDate = now,
                    LastUpdateDate = now
                });
            }
        }

        // Next two weeks: a mix of free and reserved upcoming slots.
        for (var dayOffset = 0; dayOffset < 14; dayOffset++)
        {
            var date = now.Date.AddDays(dayOffset);
            if (date.DayOfWeek is DayOfWeek.Friday)
            {
                continue;
            }

            foreach (var slot in DailySlots)
            {
                // Roughly every third upcoming slot is already booked.
                var isReserved = (dayOffset + Array.IndexOf(DailySlots, slot)) % 3 == 0;
                reservations.Add(new Reservation
                {
                    ReserveTime = date.Add(slot),
                    EndReserveTime = date.Add(slot).AddMinutes(30),
                    Reserved = isReserved,
                    CreateDate = now,
                    LastUpdateDate = now
                });
            }
        }

        context.Reservations.AddRange(reservations);
        context.SaveChanges();

        var records = new List<ReserveRecord>();
        var patientIndex = 0;
        var paymentTypes = new[] { PaymentType.Cash, PaymentType.CreditCard, PaymentType.NotPaid };

        var pastReservations = reservations.Where(r => r.ReserveTime < now).ToList();
        foreach (var reservation in pastReservations)
        {
            var patient = patients[patientIndex % patients.Count];
            patientIndex++;

            // 80% attended, 20% cancelled for realistic historical data.
            var status = patientIndex % 5 == 0 ? ReserveStatus.Cancelled : ReserveStatus.Attended;
            var paymentType = status == ReserveStatus.Cancelled ? PaymentType.NotPaid : paymentTypes[patientIndex % paymentTypes.Length];
            int? paidPrice = paymentType == PaymentType.NotPaid ? null : 50 + (patientIndex % 6) * 25;

            records.Add(new ReserveRecord
            {
                PatientId = patient.Id,
                ReservationId = reservation.Id,
                Status = status,
                PaymentType = paymentType,
                PaidPrice = paidPrice,
                Description = status == ReserveStatus.Cancelled ? "Patient cancelled the visit." : "Routine visit completed.",
                CreateDate = reservation.ReserveTime,
                LastUpdateDate = reservation.ReserveTime
            });
        }

        var upcomingReservedReservations = reservations.Where(r => r.ReserveTime >= now && r.Reserved).ToList();
        foreach (var reservation in upcomingReservedReservations)
        {
            var patient = patients[patientIndex % patients.Count];
            patientIndex++;

            records.Add(new ReserveRecord
            {
                PatientId = patient.Id,
                ReservationId = reservation.Id,
                Status = ReserveStatus.Reserved,
                PaymentType = PaymentType.NotPaid,
                Description = "Upcoming appointment.",
                CreateDate = now,
                LastUpdateDate = now
            });
        }

        context.ReserveRecords.AddRange(records);
        context.SaveChanges();

        logger.LogInformation("Seeded {ReservationCount} sample reservations and {RecordCount} sample records.", reservations.Count, records.Count);
    }
}
