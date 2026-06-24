namespace Clinic.Application.DTOs.Reservations;

public class CreateGroupReservationDto
{
    public List<DayOfWeek> DayOfWeeks { get; set; }
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public int VisitDuration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}