using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class PatientController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }  
}