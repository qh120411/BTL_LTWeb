using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class IncidentController : Controller
{
    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "Incident";
        return View();
    }
}

