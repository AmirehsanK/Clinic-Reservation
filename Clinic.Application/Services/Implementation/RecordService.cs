using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.ReserveRecords;
using Clinic.Application.Services.Interfaces;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;

namespace Clinic.Application.Services.Implementation;

public class RecordService : IRecordService
{ 
    private readonly IGenericRepository<Patient> _patientRepository;
    private readonly IGenericRepository<ReserveRecord> _reserveRecordRepository;
    private readonly IGenericRepository<Reservation> _reservationRepository;

    public RecordService(IGenericRepository<Patient> patientRepository, IGenericRepository<ReserveRecord> reserveRecordRepository, IGenericRepository<Reservation> reservationRepository)
    {
        _patientRepository = patientRepository;
        _reserveRecordRepository = reserveRecordRepository;
        _reservationRepository = reservationRepository;
    }
    public async Task<FilterRecordsDto> FilterRecords(FilterRecordsDto filter)
    {
        throw new NotImplementedException();
    }

    public async Task<ReservationRecordDetailDto> GetReservationRecordDetail(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> CreateRecord(ReserveTimeDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<EditRecordDto> GetUpdateRecord(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> UpdateRecord(EditRecordDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> DeleteRecord(int id)
    {
        throw new NotImplementedException();
    }

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await _reservationRepository.DisposeAsync();
        await _reserveRecordRepository.DisposeAsync();
        await _reservationRepository.DisposeAsync();
    }

    #endregion
}