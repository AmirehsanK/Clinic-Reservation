using Clinic.Application.Services.Interfaces;

namespace Clinic.Application.Services.Implementation;

public class OtpService :IOtpService
{
    public async ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public void GenerateOtp(string mobile)
    {
        throw new NotImplementedException();
    }

    public bool ValidateOtp(string mobile, string otp)
    {
        throw new NotImplementedException();
    }

    public bool ResendOtp(string mobile)
    {
        throw new NotImplementedException();
    }
}