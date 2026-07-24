using Clinic.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Clinic.Application.Services.Implementation;

public class OtpService(IMemoryCache cache) :IOtpService
{
    
    public void GenerateOtp(string mobile)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        cache.Set(mobile, otp,TimeSpan.FromMinutes(2));
        
    }

    public bool ValidateOtp(string mobile, string otp)
    {
        return cache.TryGetValue(mobile, out _);
    }

    public bool ResendOtp(string mobile)
    {
        return !cache.TryGetValue(mobile, out string? cachedOtp) || cachedOtp == null;
    }
    
}