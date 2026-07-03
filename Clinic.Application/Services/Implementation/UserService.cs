using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.Paging;
using Clinic.Application.DTOs.Patients;
using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Services.Implementation;

public class UserService : IUserService
{
    #region Ctor

    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Patient> _patientRepository;
    private readonly IGenericRepository<ReserveRecord> _reserveRecordRepository;

    public UserService(IGenericRepository<User> userRepository, IGenericRepository<Patient> patientRepository, IGenericRepository<ReserveRecord> reserveRecordRepository)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
    }

    #endregion

    #region User

    public async Task<List<UserDetailsDto>> GetUsersList()
    {
        var users = await _userRepository.GetAllEntities().Select(u => new UserDetailsDto
        {
            LastUpdateDate = u.LastUpdateDate,
            CreateDate = u.CreateDate,
            Id = u.Id,
            FullName = u.FullName,
            Mobile = u.Mobile
        }).ToListAsync();
        return users;
    }

    public async Task<UserDetailsDto> GetUserDetails(int id)
    {
        var user = await _userRepository.GetEntityById(id);
        return new UserDetailsDto
        {
            Id = user.Id,
            LastUpdateDate = user.LastUpdateDate,
            CreateDate = user.CreateDate,
            FullName = user.FullName,
            Mobile = user.Mobile
        };
    }

    public async Task<UpdateUserDto> GetUserForUpdate(int id)
    {
        var user = await _userRepository.GetEntityById(id);
        return new UpdateUserDto
        {
            Id = id,
            Mobile = user.Mobile,
            FullName = user.FullName,
        };
    }

    public async Task<BaseResponse> CreateUsers(CreateUserDto createUsersDto)
    {
        var dupMobile = await _userRepository.GetAllEntities().AnyAsync(u => u.Mobile == createUsersDto.Mobile);
        if (dupMobile)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "کاربر با موبایل موردنظر قبلا ثبت شده است"
            };
        }
        var user = new User()
        {
            FullName = createUsersDto.FullName,
            Mobile = createUsersDto.Mobile
        };
        await _userRepository.Create(user);
        await _userRepository.SaveChanges();
        return new BaseResponse()
        {
            Id = user.Id,
            IsSuccess = true,
            Message = "کاربر با موفقیت ثبت شد"
        };
    }

    public async Task<BaseResponse> UpdateUser(UpdateUserDto updateUsersDto)
    {
        var dupMobile = await _userRepository.GetAllEntities().AnyAsync(u => u.Mobile == updateUsersDto.Mobile && u.Id != updateUsersDto.Id);
        if (dupMobile)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "کاربر با موبایل موردنظر قبلا ثبت شده است"
            };
        }
        var user = await _userRepository.GetEntityById(updateUsersDto.Id);
        user.FullName = updateUsersDto.FullName;
        user.Mobile = updateUsersDto.Mobile;
        _userRepository.Update(user);
        await _userRepository.SaveChanges();

        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "عملیات با موفقیت انجام شد"
        };
    }

    public async Task<BaseResponse> DeleteUser(int id)
    {
        await _userRepository.Delete(id);
        await _userRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "کاربر با موفقیت حذف شد"
        };
    }

    #endregion

    #region Patient

    public async Task<FilterPatientsDto> GetPatientsList(FilterPatientsDto filterPatientsDto)
    {
        var query = _patientRepository.GetAllEntities().OrderByDescending(p => p.CreateDate).AsQueryable();

        switch (filterPatientsDto.Gender)
        {
            case FilterGender.All:
                break;
            case FilterGender.Male:
                query = query.Where(p => p.Gender == Gender.Male);
                break;
            case FilterGender.Female:
                query = query.Where(p => p.Gender == Gender.Female);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if(!string.IsNullOrEmpty(filterPatientsDto.FullName))
            query = query.Where(p => EF.Functions.Like(p.FullName.Trim(), $"%{filterPatientsDto.FullName}%"));
        if (!string.IsNullOrEmpty(filterPatientsDto.Mobile))
            query = query.Where(p => EF.Functions.Like(p.Mobile.Trim(), $"%{filterPatientsDto.Mobile}%"));
        if(!string.IsNullOrEmpty(filterPatientsDto.NationalId))
            query = query.Where(p => EF.Functions.Like(p.NationalId.Trim(), $"%{filterPatientsDto.NationalId}%"));
        if(!string.IsNullOrEmpty(filterPatientsDto.Description))
            query = query.Where(p => EF.Functions.Like(p.Description.Trim(), $"%{filterPatientsDto.Description}%"));
        if (filterPatientsDto.Age > 0)
        {
            query = query.Where(p => p.Age == filterPatientsDto.Age);
        }

        #region Paging

        var pager = Pager.Build(filterPatientsDto.PageId, await query.CountAsync(), filterPatientsDto.TakeEntity,
            filterPatientsDto.BeforeAndAfterCount);
        var allEntities = await query.Paging(pager).ToListAsync();

        #endregion
        
        return filterPatientsDto.SetData(allEntities).SetPaging(pager);
    }   

    public async Task<BaseResponse> CreatePatient(CreatePatientDto createPatientDto)
    {
        #region Validation
        
        var error= new List<string>();
        var patient = await _patientRepository.GetAllEntities().FirstOrDefaultAsync(p => p.Mobile == createPatientDto.Mobile || p.NationalId == createPatientDto.NationalId);
        if (patient != null)
        {
            if(patient.NationalId == createPatientDto.NationalId)
                error.Add($"بیمار با نام {createPatientDto.FullName} با کد ملی {createPatientDto.NationalId} قبلا ثبت شده است");
            if(patient.Mobile == createPatientDto.Mobile)
                error.Add($"بیمار با نام {createPatientDto.FullName} با موبایل {createPatientDto.Mobile} قبلا ثبت شده است");
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "عملیات با خطا مواجه شد",
                Items = error
            };
        }
        
        #endregion
        
        var newPatient = new Patient()
        {
            FullName = createPatientDto.FullName,
            Mobile = createPatientDto.Mobile,
            Age = createPatientDto.Age,
            NationalId = createPatientDto.NationalId
        };
        await _patientRepository.Create(newPatient);
        await _patientRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Id = newPatient.Id,
            Message = "عملیات با موفقیت انجام شد"
        };
    }

    public async Task<BaseResponse> CreateGroupPatients(List<CreateGroupPatientsDto> createGroupPatientsDto)
    {
        var patients = new List<Patient>();
        var errors = new List<string>();
        foreach (var item in createGroupPatientsDto)
        {
            var dupMobile = await _patientRepository.GetAllEntities().AnyAsync(p => p.Mobile == item.Mobile);
            if (dupMobile)
            {
                errors.Add($"بیمار با نام {item.FullName} با موبایل {item.Mobile} قبلا ثبت شده است");
                continue;
            }
            var dupNationalId = await _patientRepository.GetAllEntities().AnyAsync(p => p.NationalId == item.NationalId);
            if (dupNationalId)
            {
                errors.Add($"بیمار با نام {item.FullName} با کد ملی {item.NationalId} قبلا ثبت شده است");
                continue;
            }

            var patient = new Patient()
            {
                FullName = item.FullName,
                Mobile = item.Mobile,
                Age = item.Age,
                NationalId = item.NationalId,
                Gender = item.Gender
            };
            patients.Add(patient);
        }
        await _patientRepository.CreateRangeEntities(patients);
        await _patientRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "عملیات با موفقیت انجام شد",
            Items = errors
        };
    }

    public async Task<PatientDetailsDto> GetPatientDetails(int id)
    {
        var detail = await _patientRepository.GetEntityById(id);
        return new PatientDetailsDto()
        {
            Id = detail.Id,
            FullName = detail.FullName,
            Mobile = detail.Mobile,
            Age = detail.Age,
            NationalId = detail.NationalId,
            Gender = detail.Gender,
            CreateDate = detail.CreateDate,
            LastUpdateDate = detail.LastUpdateDate,
            Description = detail.Description,
            ReserveRecords = await _reserveRecordRepository.GetAllEntities().Where(r => r.PatientId== id).ToListAsync()
        };
    }

    public async Task<UpdatePatientDto> GetPatientForUpdate(int id)
    {
        var patient = await _patientRepository.GetEntityById(id);
        return new UpdatePatientDto()
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Mobile = patient.Mobile,
            Age = patient.Age,
            NationalId = patient.NationalId,
            Gender = patient.Gender
        };
    }

    public async Task<BaseResponse> UpdatePatient(UpdatePatientDto updatePatientDto)
    {
        #region Validation
        
        var error= new List<string>();
        var patient = await _patientRepository.GetAllEntities().FirstOrDefaultAsync(p => p.Id != updatePatientDto.Id && (p.Mobile == updatePatientDto.Mobile || p.NationalId == updatePatientDto.NationalId));
        if (patient != null)
        {
            if(patient.NationalId == updatePatientDto.NationalId)
                error.Add($"بیمار با نام {updatePatientDto.FullName} با کد ملی {updatePatientDto.NationalId} قبلا ثبت شده است");
            if(patient.Mobile == updatePatientDto.Mobile)
                error.Add($"بیمار با نام {updatePatientDto.FullName} با موبایل {updatePatientDto.Mobile} قبلا ثبت شده است");
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "عملیات با خطا مواجه شد",
                Items = error
            };
        }
        
        #endregion
        
        var data = await _patientRepository.GetEntityById(updatePatientDto.Id);
        data.FullName = updatePatientDto.FullName;
        data.Mobile = updatePatientDto.Mobile;
        data.Age = updatePatientDto.Age;
        data.NationalId = updatePatientDto.NationalId;
        data.Gender = updatePatientDto.Gender;
        
        _patientRepository.Update(data);
        await _patientRepository.SaveChanges();
        return new BaseResponse()
        {
            Id = data.Id,
            IsSuccess = true,
            Message = "عملیات با موفقیت انجام شد"
        };
    }

    public async Task<BaseResponse> DeletePatient(int id)
    {
        await _patientRepository.Delete(id);
        await _patientRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "بیمار با موفقیت حذف شد"
        };
    }

    #endregion

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await _userRepository.DisposeAsync();
        await _patientRepository.DisposeAsync();
        await _reserveRecordRepository.DisposeAsync();
    }

    #endregion
    
}