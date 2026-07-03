using Clinic.Application.DTOs.Common;
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

    public UserService(IGenericRepository<User> userRepository, IGenericRepository<Patient> patientRepository)
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
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> CreatePatient(CreatePatientDto createPatientDto)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> CreateGroupPatients(List<CreateGroupPatientsDto> createGroupPatientsDto)
    {
        throw new NotImplementedException();
    }

    public async Task<PatientDetailsDto> GetPatientDetails(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<UpdatePatientDto> GetPatientForUpdate(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> UpdatePatient(UpdatePatientDto updatePatientDto)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse> DeletePatient(int id)
    {
        throw new NotImplementedException();
    }

    #endregion
    

    

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await _userRepository.DisposeAsync();
        await _patientRepository.DisposeAsync();
    }

    #endregion
    
}