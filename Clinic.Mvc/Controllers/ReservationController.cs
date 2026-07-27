using Clinic.Application.DTOs.Reservations;
using Clinic.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class ReservationController(IReservationService reservationService) : BaseController
{
    #region Filters

    [HttpGet]
    public async Task<IActionResult> FilterReservation(FilterReservationsDto dto)
    {
        var data = await reservationService.FilterReservations(dto);
        return View(data);
    }

    #endregion

    #region Create Group

    [HttpGet("create-group-reservation")]
    public IActionResult CreateGroupReservation()
    {
        return View();
    }
    
    [HttpPost("create-group-reservation")]
    public async Task<IActionResult> CreateGroupReservation(CreateGroupReservationDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await reservationService.CreateGroupReservation(dto);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("FilterReservation");
        }
        TempData[ErrorMessage] = res.Message;
        return View(dto);
    }
    
    #endregion
    
    #region Create Single

    [HttpGet("create-single-reservation")]
    public IActionResult CreateReservation()
    {
        return View();
    }
    
    [HttpPost("create-single-reservation")]
    public async Task<IActionResult> CreateReservation(CreateReservationDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await reservationService.CreateReservation(dto);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("FilterReservation");
        }
        TempData[ErrorMessage] = res.Message;
        return View(dto);
    }
    
    #endregion

    #region Delete

    [Route("delete-reservation/{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var res = await reservationService.DeleteReservation(id);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("FilterReservation");
        }
        TempData[ErrorMessage] = res.Message;
        return RedirectToAction("FilterReservation");
    }

    #endregion
}