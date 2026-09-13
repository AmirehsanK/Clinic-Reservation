using Clinic.Application.DTOs.Reservations;
using Clinic.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Tests;

public class ReservationRulesTests
{
    private static readonly DateTime Tomorrow9 = DateTime.Today.AddDays(1).AddHours(9);

    [Fact]
    public async Task A_slot_that_ends_before_it_starts_is_rejected()
    {
        using var db = new TestDatabase();
        await using var context = db.NewContext();

        var result = await TestDatabase.ReservationService(context).CreateReservation(new CreateReservationDto
        {
            ReserveTime = Tomorrow9,
            EndReserveTime = Tomorrow9.AddMinutes(-30)
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await context.Reservations.CountAsync());
    }

    [Theory]
    [InlineData(0, 30)]    // identical
    [InlineData(15, 30)]   // starts inside
    [InlineData(-15, 30)]  // ends inside
    [InlineData(-15, 60)]  // wraps around
    public async Task A_slot_overlapping_an_existing_one_is_rejected(int offsetMinutes, int durationMinutes)
    {
        using var db = new TestDatabase();
        await db.AddSlot(Tomorrow9, minutes: 30);
        await using var context = db.NewContext();

        var start = Tomorrow9.AddMinutes(offsetMinutes);
        var result = await TestDatabase.ReservationService(context).CreateReservation(new CreateReservationDto
        {
            ReserveTime = start,
            EndReserveTime = start.AddMinutes(durationMinutes)
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(1, await context.Reservations.CountAsync());
    }

    [Fact]
    public async Task Back_to_back_slots_do_not_count_as_overlapping()
    {
        using var db = new TestDatabase();
        await db.AddSlot(Tomorrow9, minutes: 30);
        await using var context = db.NewContext();

        var result = await TestDatabase.ReservationService(context).CreateReservation(new CreateReservationDto
        {
            ReserveTime = Tomorrow9.AddMinutes(30),
            EndReserveTime = Tomorrow9.AddMinutes(60)
        });

        Assert.True(result.IsSuccess, result.Message);
    }

    [Fact]
    public async Task Group_generation_rejects_times_that_overlap_within_the_same_submission()
    {
        using var db = new TestDatabase();
        await using var context = db.NewContext();
        var month = DateTime.Today.AddMonths(1);

        // 09:00 and 09:15 with 30-minute visits collide on every generated day, and
        // neither is in the database yet - only the pending check can catch it.
        var result = await TestDatabase.ReservationService(context).CreateGroupReservation(new CreateGroupReservationDto
        {
            Year = month.Year,
            Month = month.Month,
            VisitDays = [DayOfWeek.Monday],
            VisitTimes = [new TimeSpan(9, 0, 0), new TimeSpan(9, 15, 0)],
            VisitDuration = 30
        });

        var mondays = Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Count(d => new DateTime(month.Year, month.Month, d).DayOfWeek == DayOfWeek.Monday);

        Assert.Equal(mondays, await context.Reservations.CountAsync());
        Assert.Equal(mondays, result.Items!.Count);
    }

    [Fact]
    public async Task A_slot_with_an_appointment_cannot_be_deleted()
    {
        using var db = new TestDatabase();
        var slotId = await db.AddSlot(Tomorrow9);
        await using (var setup = db.NewContext())
        {
            await TestDatabase.RecordService(setup).CreateReservation(new()
            {
                ReservationId = slotId, PatientName = "A", PatientMobile = "09120000001"
            });
        }

        await using var context = db.NewContext();
        var result = await TestDatabase.ReservationService(context).DeleteReservation(slotId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(await context.Reservations.SingleOrDefaultAsync(r => r.Id == slotId));
    }

    [Fact]
    public async Task Deleting_a_free_slot_soft_deletes_it_and_hides_it_from_queries()
    {
        using var db = new TestDatabase();
        var slotId = await db.AddSlot(Tomorrow9);

        await using (var context = db.NewContext())
        {
            var result = await TestDatabase.ReservationService(context).DeleteReservation(slotId);
            Assert.True(result.IsSuccess, result.Message);
        }

        await using var check = db.NewContext();
        Assert.Equal(0, await check.Reservations.CountAsync());
        var row = await check.Reservations.IgnoreQueryFilters().SingleAsync(r => r.Id == slotId);
        Assert.True(row.IsDeleted);

        // Deleting again is a clean "not found", not an exception.
        var again = await TestDatabase.ReservationService(check).DeleteReservation(slotId);
        Assert.False(again.IsSuccess);
    }
}
