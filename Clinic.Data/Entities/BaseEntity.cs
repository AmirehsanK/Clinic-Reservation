using System.ComponentModel.DataAnnotations;

namespace Clinic.Data.Entities;

public class BaseEntity
{
    [Key]
    public int Id { get; set; }

    public DateTime CreateDate { get; set; }
    
    public DateTime UpdateDate { get; set; }

    public bool IsDeleted { get; set; }
}