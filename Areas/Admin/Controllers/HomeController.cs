using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : AdminBaseController
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }
}
