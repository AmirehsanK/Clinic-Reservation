using Clinic.Application.DTOs.Reservations;
using Clinic.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

[Authorize]
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
        var now = DateTime.Now;
        var model = new CreateGroupReservationDto
        {
            Year = now.Year,
            Month = now.Month,
            VisitDays = new List<DayOfWeek>(),
            VisitTimes = new List<TimeSpan>(),
            VisitDuration = 30
        };
        return View(model);
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
            TempData[SuccessMessage] = BuildFlashMessage(res);
            return RedirectToAction("FilterReservation");
        }
        TempData[ErrorMessage] = BuildFlashMessage(res);
        return View(dto);
    }
    
    #endregion
    
    #region Create Single

    [HttpGet("create-single-reservation")]
    public IActionResult CreateReservation()
    {
        var start = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(1);
        var model = new CreateReservationDto
        {
            ReserveTime = start,
            EndReserveTime = start.AddMinutes(30)
        };
        return View(model);
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

    [HttpPost("delete-reservation/{id}")]
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

    [HttpPost("delete-group-reservation")]
    public async Task<IActionResult> DeleteGroupReservation([FromBody] List<int> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return Json(new { isSuccess = false, message = "No time slot was selected." });
        }

        var res = await reservationService.DeleteGroupReservation(ids);
        return Json(new { isSuccess = res.IsSuccess, message = res.Message, items = res.Items });
    }

    #endregion
}