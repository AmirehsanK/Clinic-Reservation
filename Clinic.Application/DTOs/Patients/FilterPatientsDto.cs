using Clinic.Application.DTOs.Paging;
using Clinic.Data.Entities;

namespace Clinic.Application.DTOs.Patients;

public class FilterPatientsDto : BasePaging
{
    public string FullName { get; set; }
    public string Mobile { get; set; }
    public string NationalId { get; set; }
    public int? Age { get; set; }
    public FilterGender Gender { get; set; }
    public string? Description { get; set; }
    
    public List<Patient> Data { get; set; }

    public FilterPatientsDto SetData(List<Patient> data)
    {
        Data = data;
        return this;
    }

    public FilterPatientsDto SetPaging(BasePaging basePaging)
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

public enum FilterGender
{
    All,
    Male,
    Female
}