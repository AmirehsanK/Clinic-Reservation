using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;
using GoogleReCaptcha.V3.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class AccountController(IUserService userService,ICaptchaValidator captchaValidator) : BaseController
{
    #region Login

    // GET
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
    #region Create User

    // GET
    [HttpGet]
    public IActionResult CreateUser()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var res= await userService.CreateUser(dto);
        if(res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("Login");
        }
        
        return View();
    }
    
    #endregion
}