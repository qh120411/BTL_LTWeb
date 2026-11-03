using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class CirculationController : Controller
{
    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "Circulation";
        return View();
    }
}

