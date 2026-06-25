using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic.Data.Entities;

public class ReserveRecord : BaseEntity
{
    public int PatientId { get; set; }
    public int ReservationId { get; set; }
    public string? Description { get; set; }
    public ReserveStatus Status { get; set; }
    public PaymentType PaymentType { get; set; }
    public int PaidPrice { get; set; }
    
    [ForeignKey("ReservationId")]
    public Reservation Reservation { get; set; }
    [ForeignKey("PatientId")]
    public Patient Patient { get; set; }
}

public enum ReserveStatus
{
    Reserved,
    Cancelled,
    Attended
}

public enum PaymentType
{
    Cash,
    CreditCard
}
