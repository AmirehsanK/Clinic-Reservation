using Clinic.Application.Services.Implementation;
using Microsoft.Extensions.Caching.Memory;

namespace Clinic.Tests;

public class OtpServiceTests
{
    private const string Mobile = "09120000000";

    private static OtpService NewService() => new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public void A_code_is_six_digits()
    {
        var code = NewService().GenerateOtp(Mobile);

        Assert.Matches("^[1-9][0-9]{5}$", code);
    }

    [Fact]
    public void A_code_works_once_and_cannot_be_replayed()
    {
        var service = NewService();
        var code = service.GenerateOtp(Mobile);

        Assert.True(service.ValidateOtp(Mobile, code));
        Assert.False(service.ValidateOtp(Mobile, code));
    }

    [Fact]
    public void A_wrong_code_is_rejected_and_does_not_burn_the_real_one()
    {
        var service = NewService();
        var code = service.GenerateOtp(Mobile);
        var wrong = code == "123456" ? "654321" : "123456";

        Assert.False(service.ValidateOtp(Mobile, wrong));
        Assert.True(service.ValidateOtp(Mobile, code));
    }

    [Fact]
    public void A_code_for_one_mobile_does_not_sign_in_another()
    {
        var service = NewService();
        var code = service.GenerateOtp(Mobile);

        Assert.False(service.ValidateOtp("09129999999", code));
    }

    [Fact]
    public void A_new_code_cannot_be_requested_while_one_is_outstanding()
    {
        var service = NewService();
        Assert.True(service.CanSendOtp(Mobile));

        service.GenerateOtp(Mobile);

        Assert.False(service.CanSendOtp(Mobile));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void A_missing_code_is_rejected(string? code)
    {
        var service = NewService();
        service.GenerateOtp(Mobile);

        Assert.False(service.ValidateOtp(Mobile, code!));
    }
}
