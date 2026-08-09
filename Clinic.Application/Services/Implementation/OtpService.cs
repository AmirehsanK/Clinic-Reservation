using System.Security.Cryptography;
using Clinic.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Clinic.Application.Services.Implementation;

public class OtpService(IMemoryCache cache) :IOtpService
{
    private const int OtpLowerBound = 100000;
    private const int OtpUpperBound = 1000000;

    public string GenerateOtp(string mobile)
    {
        // This code is the only credential in the sign-in flow, so it comes from
        // the cryptographic RNG. System.Random is seeded predictably enough that
        // codes issued close together can be guessed.
        var otp = RandomNumberGenerator.GetInt32(OtpLowerBound, OtpUpperBound).ToString();
        cache.Set(mobile, otp,TimeSpan.FromMinutes(2));
        return otp;
    }

    public bool ValidateOtp(string mobile, string otp)
    {
        if (!cache.TryGetValue(mobile, out string? cachedOtp) || string.IsNullOrEmpty(cachedOtp))
        {
            return false;
        }

        var isValid = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(cachedOtp),
            System.Text.Encoding.UTF8.GetBytes(otp ?? string.Empty));

        if (isValid)
        {
            // Prevent replay: a code can only be used once.
            cache.Remove(mobile);
        }

        return isValid;
    }

    /// <summary>
    /// True when no unexpired code is outstanding for this mobile, i.e. a new one
    /// may be sent. Named for what it answers - it does not send anything.
    /// </summary>
    public bool CanSendOtp(string mobile)
    {
        return !cache.TryGetValue(mobile, out string? cachedOtp) || cachedOtp == null;
    }
}
