using System.ComponentModel.DataAnnotations;

namespace Clinic.Application.DTOs.Users;

public class UserLoginDto
{
    [MinLength(11,ErrorMessage ="MobileNumber is too short")]
    [MaxLength(11,ErrorMessage ="MobileNumber is too long")]
    public string Mobile { get; set; }
}