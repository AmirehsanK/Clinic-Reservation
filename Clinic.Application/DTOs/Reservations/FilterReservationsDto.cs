using System.ComponentModel.DataAnnotations;
using Clinic.Application.DTOs.Paging;
using Clinic.Data.Entities;

namespace Clinic.Application.DTOs.Reservations;

public class FilterReservationsDto : BasePaging
{
    public DateTime? ReserveTime { get; set; }
    public DateTime? EndReserveTime { get; set; }
    public FilterReservationStatus FilterReservationStatus { get; set; }    
    
    public List<Reservation> Data { get; set; }

    public FilterReservationsDto SetData(List<Reservation> data)
    {
        Data = data;
        return this;
    }

    public FilterReservationsDto SetPaging(BasePaging basePaging)
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

public enum FilterReservationStatus
{
    [Display(Name = "All")]
    All,
    [Display(Name = "Reserved")]
    Reserved,
    [Display(Name = "Not Reserved")]
    NotReserved
}