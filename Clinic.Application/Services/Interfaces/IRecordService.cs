using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.ReserveRecords;

namespace Clinic.Application.Services.Interfaces;

public interface IRecordService : IAsyncDisposable
{
    Task<FilterRecordsDto> FilterRecords(FilterRecordsDto filter);
    Task<BaseResponse> CreateRecord(ReserveTimeDto dto);
    
    
}