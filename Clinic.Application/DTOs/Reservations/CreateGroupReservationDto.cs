namespace Clinic.Application.DTOs.Reservations;

public class CreateGroupReservationDto
{
    public int Year { get; set; }
    public int Month { get; set; } 
    public List<DayOfWeek> VisitDays { get; set; }
    public List<TimeSpan> VisitTimes { get; set; }
    public int VisitDuration { get; set; }
}  