using System.Data.Common;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class ReportController : AdminBaseController
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Index(string? period, string? search = null, int page = 1, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        ViewBag.ActiveMenu = "Report";
        try
        {
            var invalidDates = !ModelState.IsValid || startDate.HasValue != endDate.HasValue || startDate > endDate;
            var model = _reportService.GetReport(period, search, page, invalidDates ? null : startDate, invalidDates ? null : endDate);
            if (invalidDates)
            {
                model.LoiBoLoc = "Chọn đủ hai ngày hợp lệ; ngày bắt đầu không được sau ngày kết thúc.";
                Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            if (!invalidDates && Request.Headers["X-Report-Partial"] == "inventory")
                return PartialView("~/Views/Shared/Admin/_ReportInventory.cshtml", model);
            return View(model);
        }
        catch (Exception ex) when (ex.GetBaseException() is DbException)
        {
            _logger.LogError(ex, "Cannot read library report from SQL Server.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(new ReportViewModel { LoiDuLieu = "Không thể tải báo cáo. Vui lòng kiểm tra kết nối SQL Server." });
        }
    }

    [HttpGet]
    public IActionResult ExportExcel(string? period, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        if (!ModelState.IsValid || startDate.HasValue != endDate.HasValue || startDate > endDate)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Khoảng ngày không hợp lệ.");
        try
        {
            var fileBytes = _reportService.GenerateExcelReport(period, search, startDate, endDate);
            return File(fileBytes, "application/vnd.ms-excel", $"BaoCao_LuuThong_UTC_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
        }
        catch (Exception ex) when (ex.GetBaseException() is DbException)
        {
            _logger.LogError(ex, "Cannot export library report from SQL Server.");
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Không thể xuất báo cáo. Vui lòng kiểm tra kết nối SQL Server.");
        }
    }
}
