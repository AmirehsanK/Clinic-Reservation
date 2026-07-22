using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class ReservationController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}