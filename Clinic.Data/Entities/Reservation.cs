namespace Clinic.Data.Entities;

public class Reservation : BaseEntity
{
    public DateTime ReserveTime { get; set; }
    public DateTime EndReserveTime { get; set; }
    public bool Reserved { get; set; }    
}