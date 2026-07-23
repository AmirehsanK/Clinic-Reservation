namespace Clinic.Application.DTOs.Users;

public class AuthenticationDto
{
    public string Mobile { get; set; }
    public int OtpCode { get; set; }
}