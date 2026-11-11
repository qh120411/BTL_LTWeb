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

    // Báo cáo & thống kê theo khoảng thời gian
    [HttpGet]
    public IActionResult Index(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.ActiveMenu = "Report";
        var model = _reportService.GetReport(fromDate, toDate);
        return View(model);
    }

    // Xuất báo cáo Excel
    [HttpGet]
    public IActionResult ExportExcel(DateTime? fromDate, DateTime? toDate)
    {
        var fileBytes = _reportService.GenerateExcelReport(fromDate, toDate);
        var fileName = $"BaoCao_LuuThong_UTC_{DateTime.Now:yyyyMMdd_HHmmss}.xls";
        return File(fileBytes, "application/vnd.ms-excel", fileName);
    }
}
