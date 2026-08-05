using Clinic.Application.DTOs.Users;
using Clinic.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class AdminController(IUserService userService) : BaseController
{
    #region Admin Users List

    [HttpGet]
    public async Task<IActionResult> AdminsList()
    {
        var data = await userService.GetUsersList();
        return View(data);
    }

    #endregion
    
    #region Create Admin User
    
    [HttpGet]
    public IActionResult CreateAdmin()
    {
        return View();
    }

    
    [HttpPost]
    public async Task<IActionResult> CreateAdmin(CreateUserDto dto)
    {
    

        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        var res= await userService.CreateUser(dto);
        if(res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("AdminsList");
        }
        TempData[ErrorMessage] = res.Message;
        return View(dto);
    }
    
    #endregion
    
    #region Update Admin User
    
    [HttpGet]
    public async Task<IActionResult> UpdateAdmin(int id)
    {
        var data = await userService.GetUserForUpdate(id);
        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAdmin(UpdateUserDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        var res= await userService.UpdateUser(dto);
        if(res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("AdminsList");
        }
        TempData[ErrorMessage] = res.Message;
        return View(dto);
    }
    
    #endregion
    
    

    #region Delete Admin User
    
    [Route("delete-admin/{id}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var res= await userService.DeleteUser(id);
        if(res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("AdminsList");
        }
        TempData[ErrorMessage] = res.Message;
        return RedirectToAction("AdminsList");
    }
    
    #endregion
}