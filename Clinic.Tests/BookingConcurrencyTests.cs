using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.ReserveRecords;
using Clinic.Application.Services.Implementation;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Tests;

public class BookingConcurrencyTests
{
    private static ReserveTimeDto Booking(int slotId, string mobile) => new()
    {
        ReservationId = slotId,
        PatientName = "Patient " + mobile,
        PatientMobile = mobile
    };

    [Fact]
    public async Task Booking_a_free_slot_creates_one_record_and_marks_the_slot_reserved()
    {
        using var db = new TestDatabase();
        var slotId = await db.AddSlot(DateTime.Today.AddDays(1).AddHours(9));

        await using (var context = db.NewContext())
        {
            var result = await TestDatabase.RecordService(context).CreateReservation(Booking(slotId, "09120000001"));
            Assert.True(result.IsSuccess, result.Message);
        }

        await using var check = db.NewContext();
        Assert.True((await check.Reservations.SingleAsync(r => r.Id == slotId)).Reserved);
        Assert.Equal(1, await check.ReserveRecords.CountAsync(r => r.ReservationId == slotId));
    }

    [Fact]
    public async Task Booking_an_already_reserved_slot_is_refused()
    {
        using var db = new TestDatabase();
        var slotId = await db.AddSlot(DateTime.Today.AddDays(1).AddHours(9), reserved: true);

        await using var context = db.NewContext();
        var result = await TestDatabase.RecordService(context).CreateReservation(Booking(slotId, "09120000001"));

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await context.ReserveRecords.CountAsync());
    }

    /// <summary>
    /// The race the concurrency token exists for. Request A reads the slot while it
    /// is still free; before A writes, request B books the same slot and commits.
    /// Without the token both pass the "already reserved" check and the slot ends
    /// up with two appointments.
    /// </summary>
    [Fact]
    public async Task Two_requests_racing_for_the_same_slot_produce_exactly_one_appointment()
    {
        using var db = new TestDatabase();
        var slotId = await db.AddSlot(DateTime.Today.AddDays(1).AddHours(9));

        BaseResponse? rivalResult = null;
        await using var contextA = db.NewContext();
        var requestA = new RecordService(
            TestDatabase.Repo<Patient>(contextA),
            TestDatabase.Repo<ReserveRecord>(contextA),
            new PauseAfterReadRepository(TestDatabase.Repo<Reservation>(contextA), async () =>
            {
                await using var contextB = db.NewContext();
                rivalResult = await TestDatabase.RecordService(contextB).CreateReservation(Booking(slotId, "09120000002"));
            }));

        var resultA = await requestA.CreateReservation(Booking(slotId, "09120000001"));

        Assert.NotNull(rivalResult);
        Assert.True(rivalResult.IsSuccess, rivalResult.Message);
        Assert.False(resultA.IsSuccess);

        await using var check = db.NewContext();
        Assert.Equal(1, await check.ReserveRecords.CountAsync(r => r.ReservationId == slotId));
    }

    /// <summary>
    /// Delegates everything to the real repository, but runs <paramref name="onFirstRead"/>
    /// right after the first slot lookup - the window in which a concurrent request can
    /// get in between our check and our write.
    /// </summary>
    private sealed class PauseAfterReadRepository(IGenericRepository<Reservation> inner, Func<Task> onFirstRead)
        : IGenericRepository<Reservation>
    {
        private bool _fired;

        public async Task<Reservation> GetEntityById(int id)
        {
            var entity = await inner.GetEntityById(id);
            if (!_fired)
            {
                _fired = true;
                await onFirstRead();
            }
            return entity;
        }

        public IQueryable<Reservation> GetAllEntities() => inner.GetAllEntities();
        public Task Create(Reservation entity) => inner.Create(entity);
        public Task CreateRangeEntities(List<Reservation> entities) => inner.CreateRangeEntities(entities);
        public Task<bool> Delete(int id) => inner.Delete(id);
        public void DeleteRange(List<Reservation> entities) => inner.DeleteRange(entities);
        public void Update(Reservation entity) => inner.Update(entity);
        public Task<bool> DeletePermanently(int id) => inner.DeletePermanently(id);
        public Task SaveChanges() => inner.SaveChanges();
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
