using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.ReserveRecords;

namespace Clinic.Application.Services.Interfaces;

public interface IRecordService : IAsyncDisposable
{
    Task<FilterRecordsDto> FilterRecords(FilterRecordsDto filter);
    Task<ReservationRecordDetailDto> GetReservationRecordDetail(int id);
    Task<BaseResponse> CreateRecord(ReserveTimeDto dto);
    Task<EditRecordDto> GetUpdateRecord(int id);
    Task<BaseResponse> UpdateRecord(EditRecordDto dto);
    Task<BaseResponse> DeleteRecord(int id);
}