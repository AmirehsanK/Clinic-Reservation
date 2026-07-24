namespace Clinic.Application.Services.Interfaces;

public interface IOtpService
{
    void GenerateOtp(string mobile);
    bool ValidateOtp(string mobile, string otp);
    bool ResendOtp(string mobile);
}