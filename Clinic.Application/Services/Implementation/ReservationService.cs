using System.Runtime.InteropServices.JavaScript;
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
        var query = reservationRepository.GetAllEntities();

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
            if(createGroupReservation.VisitDays.Contains(currentDate.DayOfWeek))
            {
                continue;
            }

            foreach (var visitTime in createGroupReservation.VisitTimes)
            {
                var reserveDate = currentDate.Date.Add(visitTime);
                var endDate = reserveDate.AddMinutes(createGroupReservation.VisitDuration);

                #region Validation

                var isAvailable = await reservationRepository.GetAllEntities()
                    .AnyAsync(p => p.ReserveTime < endDate && p.EndReserveTime > reserveDate);
                
                
                if (isAvailable)
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

        var isAvailable = await reservationRepository.GetAllEntities()
            .AnyAsync(p => p.ReserveTime < createReservation.EndReserveTime && p.EndReserveTime > createReservation.ReserveTime);
        
        if (isAvailable)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "تاریخ انتخاب شده رزرو شده است.",
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
        var data = await reservationRepository.GetEntityById(reservationId);
        data.Reserved = true;
        reservationRepository.Update(data);
        await reservationRepository.SaveChanges();
    }
    public async Task CancelReservation(int reservationId)
    {
        var data = await reservationRepository.GetEntityById(reservationId);
        data.Reserved = false;
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
        await reservationRepository.Delete(reservationId);
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
            await reservationRepository.Delete(reservationId);
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