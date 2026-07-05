using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.Reservations;

namespace Clinic.Application.Services.Interfaces;

public interface IReservationService : IAsyncDisposable
{
    Task<FilterReservationsDto> FilterReservations(FilterReservationsDto filter);
    Task<BaseResponse> CreateGroupReservation(CreateGroupReservationDto createGroupReservation);
    Task<BaseResponse> CreateReservation(CreateReservationDto createReservation);
    Task<BaseResponse> DeleteReservation(int reservationId);
    Task ReserveReservation(int reservationId);
    Task CancelReservation(int reservationId);
}