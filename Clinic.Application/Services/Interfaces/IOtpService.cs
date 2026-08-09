namespace Clinic.Application.Services.Interfaces;

public interface IOtpService
{
    string GenerateOtp(string mobile);
    bool ValidateOtp(string mobile, string otp);

    /// <summary>
    /// True when no unexpired code is outstanding for this mobile.
    /// </summary>
    bool CanSendOtp(string mobile);
}
