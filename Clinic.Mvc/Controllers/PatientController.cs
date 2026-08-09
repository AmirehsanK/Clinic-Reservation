using Clinic.Application.DTOs.Patients;
using Clinic.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

[Authorize]
public class PatientController(IUserService userService) : BaseController
{
    #region Filters

    [HttpGet]
    public async Task<IActionResult> FilterPatients(FilterPatientsDto dto)
    {
        var data = await userService.GetPatientsList(dto);
        return View(data);
    }

    #endregion

    #region Create Group

    [HttpGet("create-group-patients")]
    public IActionResult CreateGroupPatients()
    {
        return View();
    }
    
    [HttpPost("create-group-patients")]
    public async Task<IActionResult> CreateGroupPatients(List<CreateGroupPatientsDto> dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await userService.CreateGroupPatients(dto);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = BuildFlashMessage(res);
            return RedirectToAction("FilterPatients");
        }
        TempData[ErrorMessage] = BuildFlashMessage(res);
        return View(dto);
    }
    
    #endregion
    
    #region Create Single

    [HttpGet("create-patient")]
    public IActionResult CreatePatient()
    {
        return View(new CreatePatientDto());
    }
    
    [HttpPost("create-patient")]
    public async Task<IActionResult> CreatePatient(CreatePatientDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await userService.CreatePatient(dto);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = BuildFlashMessage(res);
            return RedirectToAction("FilterPatients");
        }
        TempData[ErrorMessage] = BuildFlashMessage(res);
        return View(dto);
    }
    
    #endregion
    
    #region Update

    [HttpGet("edit-patient/{id}")]
    public async Task<IActionResult> EditPatient(int id)
    {
        var patient = await userService.GetPatientForUpdate(id);
        if (patient == null)
        {
            return NotFound();
        }
        return View(patient);
    }
    
    [HttpPost("edit-patient/{id}")]
    public async Task<IActionResult> EditPatient(UpdatePatientDto dto)
    {
        #region Validation

        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "Invalid Inputs.";
            return View(dto);
        }

        #endregion
        
        var res = await userService.UpdatePatient(dto);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = BuildFlashMessage(res);
            return RedirectToAction("FilterPatients");
        }
        TempData[ErrorMessage] = BuildFlashMessage(res);
        return View(dto);
    }

    #endregion

    #region Delete

    // POST, not GET: deleting is not a safe verb, and a GET route means any
    // <img>/link on another site could delete records for a signed-in admin.
    [HttpPost("delete-patient/{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var res = await userService.DeletePatient(id);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("FilterPatients");
        }
        TempData[ErrorMessage] = res.Message;
        return RedirectToAction("FilterPatients");
    }

    [HttpPost("delete-patient-with-records/{id}")]
    public async Task<IActionResult> DeletePatientWithRecords(int id)
    {
        var res = await userService.DeletePatientWithRecords(id);
        if (res.IsSuccess)
        {
            TempData[SuccessMessage] = res.Message;
            return RedirectToAction("FilterPatients");
        }
        TempData[ErrorMessage] = res.Message;
        return RedirectToAction("FilterPatients");
    }

    #endregion
}