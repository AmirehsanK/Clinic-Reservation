using System.Security.Claims;
using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

// NOTE: reCAPTCHA is registered in DI and has a config section, but no site key is
// set and the sign-in form renders no widget, so there is no token to validate.
// The validator is not injected here rather than being injected and ignored -
// wire the widget into Login.cshtml and validate here when keys are configured.
[AllowAnonymous]
public class AccountController(IUserService userService,IOtpService otpService) : BaseController
{
    #region Login
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await userService.Login(dto);
        if (!res.IsSuccess)
        {
            TempData[ErrorMessage] = res.Message;
            return View(dto);
        }
        TempData[SuccessMessage] = res.Message;
        return RedirectToAction("Authentication", new {mobile = dto.Mobile});
    }
    
    #endregion
    
    #region Authenticate User
    
    [HttpGet]
    public IActionResult Authentication(string mobile)
    {
        var model = new AuthenticationDto()
        {
            Mobile = mobile
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Authentication(AuthenticationDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await userService.CheckOtp(dto);
        if (!res.IsSuccess)
        {
            TempData[ErrorMessage] = res.Message;
            return RedirectToAction("Login");
        }

        #region Claims

        var user = await userService.GetUserDetailsByMobile(dto.Mobile);
        if (user == null)
        {
            TempData[ErrorMessage] = "Mobile Number not found.";
            return RedirectToAction("Login");
        }
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.MobilePhone, user.Mobile)
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var claimsProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };
        await HttpContext.SignInAsync(claimsPrincipal,claimsProperties);

        #endregion
        
        TempData[SuccessMessage] = res.Message;
        return RedirectToAction("Dashboard","Home");
    }
    
    #endregion

    #region Resend OTP

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(string mobile)
    {
        if(!otpService.CanSendOtp(mobile))
        {
            return Ok(new{message="Code already sent, Wait 2 minutes until try again."});
        }
        // Awaited: previously this returned "sent" before the code had been
        // generated, and swallowed any failure from the SMS gateway.
        await userService.ResendOtp(mobile);
        return Ok(new{message="Code sent successfully."});
    }

    #endregion

    #region Logout

    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        TempData[InfoMessage] = "You have been signed out successfully.";
        return RedirectToAction("Login");
    }

    #endregion
}