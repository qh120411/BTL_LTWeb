using BTL_LTWEB.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : AdminBaseController
{
    private readonly IReaderService _readerService;

    public DashboardController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    public IActionResult Index()
    {
        ViewBag.ActiveMenu = "Dashboard";
        var model = _readerService.GetDashboardStatistics();
        return View("~/Views/Home/Admin/Index.cshtml", model);
    }
}
