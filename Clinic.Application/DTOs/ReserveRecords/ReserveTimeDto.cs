namespace Clinic.Application.DTOs.ReserveRecords;

public class ReserveTimeDto
{
    public int ReservationId { get; set; }
    public string PatientName { get; set; }
    public string PatientMobile { get; set; }
    public string? NationalId { get; set; }
}