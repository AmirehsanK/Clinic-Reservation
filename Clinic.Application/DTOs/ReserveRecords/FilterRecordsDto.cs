using System.ComponentModel.DataAnnotations;
using Clinic.Application.DTOs.Paging;
using Clinic.Data.Entities;

namespace Clinic.Application.DTOs.ReserveRecords;

public class FilterRecordsDto : BasePaging
{
    public string PatientNationalId { get; set; }
    public string PatientName { get; set; }
    public int? PatientId { get; set; }
    public int? ReservationId { get; set; }
    public string? Description { get; set; }
    public FilterRecordStatus Status { get; set; }
    public FilterPaymentType PaymentType { get; set; }
    public int? PaidPrice { get; set; }
    public List<ReserveRecord> Data { get; set; }

    public FilterRecordsDto SetData(List<ReserveRecord> data)
    {
        Data = data;
        return this;
    }

    public FilterRecordsDto SetPaging(BasePaging basePaging)
    {
        PageId=basePaging.PageId;
        AllEntitiesCount=basePaging.AllEntitiesCount;
        StartPage=basePaging.StartPage;
        EndPage=basePaging.EndPage;
        BeforeAndAfterCount=basePaging.BeforeAndAfterCount;
        TakeEntity=basePaging.TakeEntity;
        SkipEntity=basePaging.SkipEntity;
        PageCount=basePaging.PageCount;
        
        return this;
    }
}
public enum FilterRecordStatus
{
    [Display(Name = "All")]
    All,
    [Display(Name = "Reserved")]
    Reserved,
    [Display(Name = "Cancelled")]
    Cancelled,
    [Display(Name = "Attended")]
    Attended
}
    
public enum FilterPaymentType
{
    [Display(Name = "All")]
    All,
    [Display(Name = "Cash")]
    Cash,
    [Display(Name = "Credit Card")]
    CreditCard,
    [Display(Name = "Not Paid")]
    NotPaid
}