namespace Clinic.Application.DTOs.Common;

public class BaseResponse
{
    public int? Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    
    public List<string>? Items { get; set; }
}