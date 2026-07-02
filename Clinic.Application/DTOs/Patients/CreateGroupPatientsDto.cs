using Clinic.Data.Entities;

namespace Clinic.Application.DTOs.Patients;

public class CreateGroupPatientsDto
{
    public string FullName { get; set; }
    public string Mobile { get; set; }
    public string NationalId { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
}