using Clinic.Data.Entities;

namespace Clinic.Application.DTOs.ReserveRecords;

public class EditRecordDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public ReserveStatus Status { get; set; }
    public PaymentType PaymentType { get; set; }
    public int PaidPrice { get; set; }
}