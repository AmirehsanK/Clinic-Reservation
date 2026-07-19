using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.ReserveRecords;
using Clinic.Application.Services.Interfaces;

namespace Clinic.Application.Services.Implementation;

public class RecordService : IRecordService
{

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
        throw new NotImplementedException();
    }

    #endregion
}