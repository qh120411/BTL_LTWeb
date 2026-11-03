using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class OverdueController : Controller
{
    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "Overdue";
        return View();
    }
}

