using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class UserController : Controller
{
    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "User";
        return View();
    }
}

