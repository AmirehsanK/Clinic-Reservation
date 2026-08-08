using Clinic.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clinic.Application.Services.Implementation;

public class SmsService(ILogger<SmsService> logger) : ISmsService
{
    public Task SendOtp(string mobile, string otp)
    {
        // TODO: Replace with a real SMS gateway integration for production use.
        // Logging the code keeps local development and testing functional out of the box.
        logger.LogInformation("OTP for {Mobile}: {Otp}", mobile, otp);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
