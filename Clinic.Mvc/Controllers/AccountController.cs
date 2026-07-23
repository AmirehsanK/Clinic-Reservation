using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;
using GoogleReCaptcha.V3.Interface;
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
        
        return View();
    }
    
    #endregion
    
}