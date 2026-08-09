using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.Paging;
using Clinic.Application.DTOs.Reservations;
using Clinic.Application.Services.Interfaces;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Services.Implementation;

public class ReservationService(IGenericRepository<Reservation> reservationRepository,IGenericRepository<ReserveRecord> recordRepository) : IReservationService
{
    
    public async Task<FilterReservationsDto> FilterReservations(FilterReservationsDto filter)
    {
        // Ordered before paging: Skip/Take over an unordered query gives the
        // database licence to return rows in any order, so pages could repeat or
        // omit slots between requests.
        var query = reservationRepository.GetAllEntities().OrderBy(r => r.ReserveTime).ThenBy(r => r.Id).AsQueryable();

        switch (filter.FilterReservationStatus)
        {
            case FilterReservationStatus.All:
                break;
            case FilterReservationStatus.Reserved:
                query = query.Where(p => p.Reserved);
                break;
            case FilterReservationStatus.NotReserved:
                query = query.Where(p => !p.Reserved);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if(filter.ReserveTime != null)
            query = query.Where(p => p.ReserveTime.Date >= filter.ReserveTime.Value.Date);
        
        #region Paging

        var pager = Pager.Build(filter.PageId, await query.CountAsync(), filter.TakeEntity,
            filter.BeforeAndAfterCount);
        var allEntities = await query.Paging(pager).ToListAsync();

        #endregion
        
        return filter.SetData(allEntities).SetPaging(pager);
    }

    public async Task<BaseResponse> CreateGroupReservation(CreateGroupReservationDto createGroupReservation)
    {
        int daysInMonth = DateTime.DaysInMonth(createGroupReservation.Year, createGroupReservation.Month);
        var reservations = new List<Reservation>();
        var errors = new List<string>();
        for (var day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateTime(createGroupReservation.Year, createGroupReservation.Month, day);
            if(!createGroupReservation.VisitDays.Contains(currentDate.DayOfWeek))
            {
                continue;
            }

            foreach (var visitTime in createGroupReservation.VisitTimes)
            {
                var reserveDate = currentDate.Date.Add(visitTime);
                var endDate = reserveDate.AddMinutes(createGroupReservation.VisitDuration);

                #region Validation

                var overlapsExisting = await reservationRepository.GetAllEntities()
                    .AnyAsync(p => p.ReserveTime < endDate && p.EndReserveTime > reserveDate);

                // Slots queued earlier in this same request are not in the database
                // yet, so they have to be checked separately - otherwise submitting
                // two overlapping visit times creates both.
                var overlapsPending = reservations
                    .Any(p => p.ReserveTime < endDate && p.EndReserveTime > reserveDate);

                if (overlapsExisting || overlapsPending)
                {
                    errors.Add($"Date {reserveDate} to {endDate} is already reserved.");
                    continue;
                }

                #endregion

                reservations.Add(new Reservation
                {
                    ReserveTime = reserveDate,
                    EndReserveTime = endDate,
                    Reserved = false
                });
            }
        }
        await reservationRepository.CreateRangeEntities(reservations);
        await reservationRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "Task completed successfully.",
            Items = errors
        };
    }

    public async Task<BaseResponse> CreateReservation(CreateReservationDto createReservation)
    {
        #region Validation

        if (createReservation.EndReserveTime <= createReservation.ReserveTime)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "The end time must be after the start time.",
            };
        }

        var overlaps = await reservationRepository.GetAllEntities()
            .AnyAsync(p => p.ReserveTime < createReservation.EndReserveTime && p.EndReserveTime > createReservation.ReserveTime);

        if (overlaps)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "The selected time is already reserved.",
            };
        }

        #endregion

        var reservation = new Reservation
        {
            ReserveTime = createReservation.ReserveTime,
            Reserved = false,
            EndReserveTime = createReservation.EndReserveTime
        };
        await reservationRepository.Create(reservation);
        await reservationRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "Task completed successfully.",
        };
    }
    
    public async Task ReserveReservation(int reservationId)
    {
        await SetReservedFlag(reservationId, true);
    }
    public async Task CancelReservation(int reservationId)
    {
        await SetReservedFlag(reservationId, false);
    }

    private async Task SetReservedFlag(int reservationId, bool reserved)
    {
        var data = await reservationRepository.GetEntityById(reservationId);
        if (data == null)
        {
            return;
        }
        data.Reserved = reserved;
        reservationRepository.Update(data);
        await reservationRepository.SaveChanges();
    }
    public async Task<BaseResponse> DeleteReservation(int reservationId)
    {
        #region Validation

        var usage = await recordRepository.GetAllEntities()
            .AnyAsync(p => p.ReservationId == reservationId);
        if (usage)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Reservation is in use and cannot be deleted.",
            };
        }

        #endregion

        if (!await reservationRepository.Delete(reservationId))
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Time slot not found.",
            };
        }
        await reservationRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "Task completed successfully.",
        };
    }
    public async Task<BaseResponse> DeleteGroupReservation(List<int> reservationIds)
    {
        var errors = new List<string>();
        foreach (var reservationId in reservationIds)
        {
            var usage = await recordRepository.GetAllEntities()
                .AnyAsync(p => p.ReservationId == reservationId);
            if (usage)
            {
                errors.Add($"Reservation with ID {reservationId} is in use and cannot be deleted.");
                continue;
            }
            if (!await reservationRepository.Delete(reservationId))
            {
                errors.Add($"Reservation with ID {reservationId} was not found.");
            }
        }
        await reservationRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "Task completed successfully.",
            Items = errors
        };
    }
    
    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await reservationRepository.DisposeAsync();
        await recordRepository.DisposeAsync();
    }

    #endregion
}