using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class CatalogController : Controller
{
    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "Catalog";
        return View();
    }
}

