using Clinic.Application.DTOs.Common;
using Clinic.Application.DTOs.Patients;
using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;

namespace Clinic.Application.Services.Implementation;

public class UserService : IUserService
{
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public Task<UsersListDto> GetUsersList()
    {
        throw new NotImplementedException();
    }

    public Task<UserDetailsDto> GetUserDetails(int id)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateUserDto> GetUserForUpdate(int id)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> CreateUsers(CreateUserDto createUsersDto)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> UpdateUser(UpdateUserDto updateUsersDto)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> DeleteUser(int id)
    {
        throw new NotImplementedException();
    }

    public Task<FilterPatientsDto> GetPatientsList(FilterPatientsDto filterPatientsDto)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> CreatePatient(CreatePatientDto createPatientDto)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> CreateGroupPatients(List<CreateGroupPatientsDto> createGroupPatientsDto)
    {
        throw new NotImplementedException();
    }

    public Task<PatientDetailsDto> GetPatientDetails(int id)
    {
        throw new NotImplementedException();
    }

    public Task<UpdatePatientDto> GetPatientForUpdate(int id)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> UpdatePatient(UpdatePatientDto updatePatientDto)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResponse> DeletePatient(int id)
    {
        throw new NotImplementedException();
    }
}