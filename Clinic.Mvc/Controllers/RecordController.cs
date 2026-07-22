using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class RecordController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}