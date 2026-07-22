using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}