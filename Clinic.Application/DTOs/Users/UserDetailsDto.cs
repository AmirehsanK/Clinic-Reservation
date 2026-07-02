namespace Clinic.Application.DTOs.Users;

public class UserDetailsDto
{
    public int Id { get; set; }

    public DateTime CreateDate { get; set; }
    
    public DateTime LastUpdateDate { get; set; }
    
    public string FullName { get; set; }
    
    public string Mobile { get; set; }
}