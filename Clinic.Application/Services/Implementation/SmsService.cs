using Clinic.Application.Services.Interfaces;

namespace Clinic.Application.Services.Implementation;

public class SmsService: ISmsService
{
    public async Task SendOtp(string mobile, string otp)
    {
        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}