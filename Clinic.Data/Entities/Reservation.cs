namespace Clinic.Data.Entities;

public class Reservation : BaseEntity
{
    public DateTime ReserveTime { get; set; }
    public bool IsReserved { get; set; }    
    
}