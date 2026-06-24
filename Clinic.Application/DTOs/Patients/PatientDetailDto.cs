using Clinic.Data.Entities;

namespace Clinic.Application.DTOs.Patients;

public class PatientDetailDto
{
    public int Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime LastUpdateDate { get; set; }
    
    public string FullName { get; set; }
    public string Mobile { get; set; }
    public string NationalId { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? Description { get; set; }

    public List<ReserveRecord> ReserveRecords { get; set; } = [];
}