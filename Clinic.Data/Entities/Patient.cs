namespace Clinic.Data.Entities;

public class Patient : BaseEntity
{
    public string FullName { get; set; }
    public string Mobile { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? Description { get; set; }
}
public enum Gender
{
    Male,
    Female
}