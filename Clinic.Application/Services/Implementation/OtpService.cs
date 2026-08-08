using Clinic.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Clinic.Application.Services.Implementation;

public class OtpService(IMemoryCache cache) :IOtpService
{
    
    public string GenerateOtp(string mobile)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        cache.Set(mobile, otp,TimeSpan.FromMinutes(2));
        return otp;
    }

    public bool ValidateOtp(string mobile, string otp)
    {
        if (!cache.TryGetValue(mobile, out string? cachedOtp) || string.IsNullOrEmpty(cachedOtp))
        {
            return false;
        }

        var isValid = cachedOtp == otp;
        if (isValid)
        {
            // Prevent replay: a code can only be used once.
            cache.Remove(mobile);
        }

        return isValid;
    }

    public bool ResendOtp(string mobile)
    {
        return !cache.TryGetValue(mobile, out string? cachedOtp) || cachedOtp == null;
    }
    
}