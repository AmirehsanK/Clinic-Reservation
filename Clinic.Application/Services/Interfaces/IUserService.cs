using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.Patients;
using Clinic.Application.DTOs.Users;

namespace Clinic.Application.Services.Interfaces;

public interface IUserService : IAsyncDisposable
{
    #region Users

    Task<UsersListDto> GetUsersList();
    Task<UserDetailsDto> GetUserDetails(int id);
    Task<UpdateUserDto> GetUserForUpdate(int id);
    Task<BaseResponse> CreateUsers(CreateUserDto createUsersDto);
    Task<BaseResponse> UpdateUser(UpdateUserDto updateUsersDto);
    Task <BaseResponse> DeleteUser(int id);
    
    #endregion

    #region Patients

    Task<FilterPatientsDto> GetPatientsList(FilterPatientsDto filterPatientsDto);
    Task<BaseResponse> CreatePatient(CreatePatientDto createPatientDto);
    Task<BaseResponse> CreateGroupPatients(List<CreateGroupPatientsDto> createGroupPatientsDto);
    Task<PatientDetailsDto> GetPatientDetails(int id);
    Task<UpdatePatientDto> GetPatientForUpdate(int id);
    Task<BaseResponse> UpdatePatient(UpdatePatientDto updatePatientDto);
    Task<BaseResponse> DeletePatient(int id);
    
    #endregion
}