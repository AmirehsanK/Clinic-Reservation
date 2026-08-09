using Clinic.Application.DTOs.ReserveRecords;
using Clinic.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

[Authorize]
public class RecordController(IRecordService recordService) : BaseController
{
    #region Filter / List

    [HttpGet]
    public async Task<IActionResult> Index(FilterRecordsDto dto)
    {
        var data = await recordService.FilterRecords(dto);
        return View(data);
    }

    #endregion

    #region Update

    [HttpGet("edit-record/{id}")]
    public async Task<IActionResult> EditRecord(int id)
    {
        var data = await recordService.GetUpdateRecord(id);
        if (data == null)
        {
            return NotFound();
        }
        return View(data);
    }

    [HttpPost("edit-record/{id}")]
    public async Task<IActionResult> EditRecord(EditRecordDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion

        var res = await recordService.UpdateRecord(dto);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("Index");
        }
        TempData[ErrorMessage] = res.Message;
        return View(dto);
    }

    #endregion

    #region Delete

    [HttpPost("delete-record/{id}")]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        var res = await recordService.DeleteRecord(id);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("Index");
        }
        TempData[ErrorMessage] = res.Message;
        return RedirectToAction("Index");
    }

    #endregion
}
