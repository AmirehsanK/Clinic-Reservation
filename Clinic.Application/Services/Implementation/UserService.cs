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
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;

    public UserService(IGenericRepository<User> userRepository, IGenericRepository<Patient> patientRepository, IGenericRepository<ReserveRecord> reserveRecordRepository, IOtpService otpService, ISmsService smsService)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _reserveRecordRepository = reserveRecordRepository;
        _otpService = otpService;
        _smsService = smsService;
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

    public async Task<UserDetailsDto> GetUserDetailsById(int id)
    {
        var user = await _userRepository.GetEntityById(id);
        if (user == null)
        {
            return null;
        }
        return new UserDetailsDto
        {
            Id = user.Id,
            LastUpdateDate = user.LastUpdateDate,
            CreateDate = user.CreateDate,
            FullName = user.FullName,
            Mobile = user.Mobile
        };
    }
    public async Task<UserDetailsDto> GetUserDetailsByMobile(string mobile)
    {
        var user = await _userRepository.GetAllEntities().FirstOrDefaultAsync(u => u.Mobile == mobile);
        if (user == null)
        {
            return null;
        }
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
        if (user == null)
        {
            return null;
        }
        return new UpdateUserDto
        {
            Id = id,
            Mobile = user.Mobile,
            FullName = user.FullName,
        };
    }

    public async Task<BaseResponse> CreateUser(CreateUserDto createUsersDto)
    {
        var dupMobile = await _userRepository.GetAllEntities().AnyAsync(u => u.Mobile == createUsersDto.Mobile);
        if (dupMobile)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "User with the Phone Number  already exists."
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
            Message = "User created successfully."
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
                Message = "User with the Phone Number  already exists."
            };
        }
        var user = await _userRepository.GetEntityById(updateUsersDto.Id);
        if (user == null)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "User not found."
            };
        }
        user.FullName = updateUsersDto.FullName;
        user.Mobile = updateUsersDto.Mobile;
        _userRepository.Update(user);
        await _userRepository.SaveChanges();

        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "Operation completed successfully."
        };
    }

    public async Task<BaseResponse> DeleteUser(int id)
    {
        if (!await _userRepository.Delete(id))
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "User not found."
            };
        }
        await _userRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = "User deleted successfully."
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
            case FilterGender.NotSpecified:
                query = query.Where(p => p.Gender == Gender.NotSpecified);
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
                error.Add($"Patient with name {createPatientDto.FullName} and national ID {createPatientDto.NationalId} already exists");
            if(patient.Mobile == createPatientDto.Mobile)
                error.Add($"Patient with name {createPatientDto.FullName} and mobile {createPatientDto.Mobile} already exists");
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Operation failed",
                Items = error
            };
        }
        
        #endregion
        
        var newPatient = new Patient()
        {
            FullName = createPatientDto.FullName,
            Mobile = createPatientDto.Mobile,
            Age = createPatientDto.Age,
            NationalId = createPatientDto.NationalId,
            Gender = createPatientDto.Gender,
            Description = createPatientDto.Description
        };
        await _patientRepository.Create(newPatient);
        await _patientRepository.SaveChanges();
        return new BaseResponse()
        {
            IsSuccess = true,
            Id = newPatient.Id,
            Message = "Operation completed successfully."
        };
    }

    public async Task<BaseResponse> CreateGroupPatients(List<CreateGroupPatientsDto> createGroupPatientsDto)
    {
        var patients = new List<Patient>();
        var errors = new List<string>();

        // Pull the existing mobiles/national IDs once instead of two queries per
        // submitted row, then grow the same sets as rows are accepted so that
        // duplicates *within* this batch are caught too - checking only against
        // the database would let two identical rows in one submission through.
        var submittedMobiles = createGroupPatientsDto.Select(p => p.Mobile).ToList();
        var submittedNationalIds = createGroupPatientsDto.Select(p => p.NationalId).ToList();

        var takenMobiles = (await _patientRepository.GetAllEntities()
            .Where(p => submittedMobiles.Contains(p.Mobile))
            .Select(p => p.Mobile)
            .ToListAsync()).ToHashSet();
        var takenNationalIds = (await _patientRepository.GetAllEntities()
            .Where(p => submittedNationalIds.Contains(p.NationalId))
            .Select(p => p.NationalId)
            .ToListAsync()).ToHashSet();

        foreach (var item in createGroupPatientsDto)
        {
            if (takenMobiles.Contains(item.Mobile))
            {
                errors.Add($"Patient with name {item.FullName} and mobile {item.Mobile} already exists");
                continue;
            }
            if (takenNationalIds.Contains(item.NationalId))
            {
                errors.Add($"Patient with name {item.FullName} and national ID {item.NationalId} already exists");
                continue;
            }

            takenMobiles.Add(item.Mobile);
            takenNationalIds.Add(item.NationalId);

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
            Message = "Operation completed successfully.",
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
        if (patient == null)
        {
            return null;
        }
        return new UpdatePatientDto()
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Mobile = patient.Mobile,
            Age = patient.Age,
            NationalId = patient.NationalId,
            Gender = patient.Gender,
            Description = patient.Description
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
                error.Add($"Patient with name {updatePatientDto.FullName} and national ID {updatePatientDto.NationalId} already exists");
            if(patient.Mobile == updatePatientDto.Mobile)
                error.Add($"Patient with name {updatePatientDto.FullName} and mobile {updatePatientDto.Mobile} already exists");
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Operation failed",
                Items = error
            };
        }
        
        #endregion
        
        var data = await _patientRepository.GetEntityById(updatePatientDto.Id);
        if (data == null)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Patient not found."
            };
        }
        data.FullName = updatePatientDto.FullName;
        data.Mobile = updatePatientDto.Mobile;
        data.Age = updatePatientDto.Age;
        data.NationalId = updatePatientDto.NationalId;
        data.Gender = updatePatientDto.Gender;
        data.Description = updatePatientDto.Description;

        _patientRepository.Update(data);
        await _patientRepository.SaveChanges();
        return new BaseResponse()
        {
            Id = data.Id,
            IsSuccess = true,
            Message = "Operation completed successfully."
        };
    }

    public async Task<BaseResponse> DeletePatient(int id)
    {
        var hasRecord = await _reserveRecordRepository.GetAllEntities().AnyAsync(r => r.PatientId == id);
        if (hasRecord)
        {
            return new BaseResponse
            {
                IsSuccess = false,
                Message = "Cannot delete patient with existing reservation records."
            };
        }
        if (!await _patientRepository.Delete(id))
        {
            return new BaseResponse
            {
                IsSuccess = false,
                Message = "Patient not found."
            };
        }
        await _patientRepository.SaveChanges();
        return new BaseResponse
        {
            IsSuccess = true,
            Message = "Patient deleted successfully."
        };
    }

    public async Task<BaseResponse> DeletePatientWithRecords(int id)
    {
        var patientExists = await _patientRepository.GetAllEntities().AnyAsync(p => p.Id == id);
        if (!patientExists)
        {
            return new BaseResponse
            {
                IsSuccess = false,
                Message = "Patient not found."
            };
        }

        var records = await _reserveRecordRepository.GetAllEntities().Where(r => r.PatientId == id).ToListAsync();
        _reserveRecordRepository.DeleteRange(records);
        await _reserveRecordRepository.SaveChanges();

        await _patientRepository.Delete(id);
        await _patientRepository.SaveChanges();
        return new BaseResponse
        {
            IsSuccess = true,
            Message = "Patient and all related records deleted successfully."
        };
    }

    #endregion

    #region Authentication

    public async Task ResendOtp(string mobile)
    {
        var otp = _otpService.GenerateOtp(mobile);
        await _smsService.SendOtp(mobile, otp);
    }

    public async Task<BaseResponse> CheckOtp(AuthenticationDto dto)
    {
        var user =  await _userRepository.GetAllEntities().FirstOrDefaultAsync(u => u.Mobile == dto.Mobile);
        if (user==null)
            return new BaseResponse
            {
                IsSuccess = false,
                Message = "Mobile Number not found."
            };
        var result = _otpService.ValidateOtp(dto.Mobile, dto.OtpCode);
        if (result)
        {
            return new BaseResponse
            {
                IsSuccess = true,
                Message = "Login successful."
            };
        }
        
        return new BaseResponse
            {
                IsSuccess = false,
                Message = "Invalid OTP."
            };
    }

    public async Task<BaseResponse> Login(UserLoginDto dto)
    {
        var numberExist = await _userRepository.GetAllEntities().AnyAsync(u => u.Mobile == dto.Mobile);
        if (!numberExist)
            return new BaseResponse
            {
                IsSuccess = false,
                Message = "Mobile Number not found."
            };
        if (!_otpService.CanSendOtp(dto.Mobile))
            return new BaseResponse
            {
                IsSuccess = false,
                Message = "Wait 2 minutes before requesting a new OTP."
            };
        var otp = _otpService.GenerateOtp(dto.Mobile);
        // SendOtp's second argument is the code itself, not a message - the SMS
        // gateway owns the wording. ResendOtp passes it the same way.
        await _smsService.SendOtp(dto.Mobile, otp);
        return new BaseResponse
        {
            IsSuccess = true,
            Message = "OTP sent successfully."
        };
    }

    #endregion

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await _userRepository.DisposeAsync();
        await _patientRepository.DisposeAsync();
        await _reserveRecordRepository.DisposeAsync();
        await _smsService.DisposeAsync();
    }

    #endregion
    
}