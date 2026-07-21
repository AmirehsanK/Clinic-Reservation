using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.Paging;
using Clinic.Application.DTOs.ReserveRecords;
using Clinic.Application.Services.Interfaces;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Services.Implementation;

public class RecordService : IRecordService
{
    #region Ctor

    private readonly IGenericRepository<Patient> _patientRepository;
    private readonly IGenericRepository<ReserveRecord> _recordRepository;
    private readonly IGenericRepository<Reservation> _reservationRepository;

    public RecordService(IGenericRepository<Patient> patientRepository, IGenericRepository<ReserveRecord> reserveRecordRepository, IGenericRepository<Reservation> reservationRepository)
    {
        _patientRepository = patientRepository;
        _recordRepository = reserveRecordRepository;
        _reservationRepository = reservationRepository;
    }

    #endregion
    
    public async Task<FilterRecordsDto> FilterRecords(FilterRecordsDto filter)
    {
        var query = _recordRepository.GetAllEntities().Include(r=>r.Patient)
            .OrderByDescending(p => p.CreateDate).AsQueryable();

        #region Switch

        switch (filter.PaymentType)
        {
            case FilterPaymentType.All:
                break;
            case FilterPaymentType.Cash:
                query = query.Where(p => p.PaymentType == PaymentType.Cash); break;
            case FilterPaymentType.CreditCard:
                query = query.Where(p => p.PaymentType == PaymentType.CreditCard); break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (filter.Status)
        {
            case FilterRecordStatus.All:
                break;
            case FilterRecordStatus.Reserved:
                query = query.Where(p => p.Status == ReserveStatus.Reserved); break;
            case FilterRecordStatus.Cancelled:
                query = query.Where(p => p.Status == ReserveStatus.Cancelled); break;
            case FilterRecordStatus.Attended:
                query = query.Where(p => p.Status == ReserveStatus.Attended); break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        #endregion

        #region Filter

        if(!string.IsNullOrEmpty(filter.PatientName))
            query = query.Where(p => EF.Functions.Like(p.Patient.FullName.Trim(), $"%{filter.PatientName}%"));
        if(!string.IsNullOrEmpty(filter.PatientNationalId))
            query = query.Where(p => EF.Functions.Like(p.Patient.NationalId.Trim(), $"%{filter.PatientNationalId}%"));
        if(!string.IsNullOrEmpty(filter.Description))
            query = query.Where(p => EF.Functions.Like(p.Description.Trim(), $"%{filter.Description}%"));
        
        if (filter.PatientId > 0)
        {
            query = query.Where(p => p.PatientId == filter.PatientId);
        }
        if (filter.ReservationId is > 0)
        {
            query = query.Where(p => p.ReservationId == filter.ReservationId.Value);
        }
        if (filter.PaidPrice is > 0)
        {
            query = query.Where(p => p.PaidPrice == filter.PaidPrice.Value);
        }
        
        #endregion
        
        #region Paging

        var pager = Pager.Build(filter.PageId, await query.CountAsync(), filter.TakeEntity,
            filter.BeforeAndAfterCount);
        var allEntities = await query.Paging(pager).ToListAsync();

        #endregion
        
        return filter.SetData(allEntities).SetPaging(pager);
    }

    public async Task<ReservationRecordDetailDto> GetReservationRecordDetail(int id)
    {
        var data = await _recordRepository.GetEntityById(id);
        return new ReservationRecordDetailDto
        {
            Id = data.Id,
            Description = data.Description,
            CreateDate = data.CreateDate,
            Patient = await _patientRepository.GetEntityById(data.PatientId),
            PatientId = data.PatientId,
            LastUpdateDate = data.LastUpdateDate,
            Reservation = await _reservationRepository.GetEntityById(data.ReservationId),
            Status = data.Status,
            ReservationId = data.ReservationId,
            PaidPrice = data.PaidPrice,
            PaymentType = data.PaymentType
        };
    }

    public async Task<BaseResponse> CreateReservation(ReserveTimeDto dto)
    {
        #region Validation

        var availablity = await _reservationRepository.GetEntityById(dto.ReservationId);
        if (availablity.Reserved)
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "نوبت رزرو شده است."
            };

        #endregion
        
        var patient= await _patientRepository.GetAllEntities().FirstOrDefaultAsync(p => p.Mobile == dto.PatientMobile);
        if (patient is null)
        {
            var newPatient = new Patient()
            {
                Age = 0,
                Gender = Gender.NotSpecified,
                FullName = dto.PatientName,
                Mobile = dto.PatientMobile,
                NationalId = dto.NationalId ?? "NotSpecified"
    
            };
            await _patientRepository.Create(newPatient);
            await _patientRepository.SaveChanges();
            patient=newPatient;
        }
        
        var reservation = new ReserveRecord
        {
            PatientId = patient.Id,
            Status = ReserveStatus.Reserved,
            PaymentType = PaymentType.NotPaid,
            ReservationId = dto.ReservationId
        };
        
        await _recordRepository.Create(reservation);
        await _recordRepository.SaveChanges();
        return new BaseResponse
        {
            Id = reservation.Id,
            IsSuccess = true,
            Message = "نوبت با موفقیت رزرو شد."
        };
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
        await _recordRepository.DisposeAsync();
        await _patientRepository.DisposeAsync();
    }

    #endregion
}