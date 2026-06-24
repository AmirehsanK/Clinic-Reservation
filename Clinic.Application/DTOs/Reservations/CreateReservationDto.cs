namespace Clinic.Application.DTOs.Reservations;

public class CreateReservationDto
{
    public DateTime ReserveTime { get; set; }
    public DateTime EndReserveTime { get; set; }
    public bool Reserved { get; set; }  
}