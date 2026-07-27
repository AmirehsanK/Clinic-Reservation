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

    #region Create

    [HttpGet("create-reservation")]
    public IActionResult CreateGroupReservation()
    {
        return View();
    }
    
    [HttpPost("create-reservation")]
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
}