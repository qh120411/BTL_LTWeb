using Microsoft.AspNetCore.Mvc;
using BTL_LTWEB.Areas.Admin.Services;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class ReportController : AdminBaseController
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Figma 07: Báo cáo & thống kê
    [HttpGet]
    public IActionResult Index(string? period, string? scope)
    {
        ViewBag.ActiveMenu = "Report";
        var model = _reportService.GetReport(period, scope);
        return View(model);
    }

    // Xuất báo cáo Excel
    [HttpGet]
    public IActionResult ExportExcel(string? period, string? scope)
    {
        var fileBytes = _reportService.GenerateExcelReport(period, scope);
        var fileName = $"BaoCao_LuuThong_UTC_{DateTime.Now:yyyyMMdd_HHmmss}.xls";
        return File(fileBytes, "application/vnd.ms-excel", fileName);
    }
}

