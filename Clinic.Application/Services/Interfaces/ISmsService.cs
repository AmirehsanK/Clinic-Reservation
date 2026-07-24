namespace Clinic.Application.Services.Interfaces;

public interface ISmsService : IAsyncDisposable
{
    Task SendOtp(string mobile, string otp);
}