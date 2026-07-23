using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;
using GoogleReCaptcha.V3.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class AccountController(IUserService userService,ICaptchaValidator captchaValidator) : BaseController
{
    #region Login
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(UserLoginDto dto)
    {
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
        //Otp
        return View();
    }
    
    #endregion

    #region Resend OTP

    [HttpPost("resend-otp")]
    public IActionResult ResendOTP(string mobile)
    {
        //resend otp
        return Ok();
    }

    #endregion

    #region Logout

    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        TempData[InfoMessage] = "شما با موفقیت از حساب کاربری خود خارج شدید.";
        return RedirectToAction("Login");
    }

    #endregion
}