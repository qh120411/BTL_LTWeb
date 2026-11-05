using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class UserController : AdminBaseController
{
    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "User";
        return View();
    }
}

