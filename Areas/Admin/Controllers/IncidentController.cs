using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class IncidentController : AdminBaseController
{
    private readonly IReaderService _readerService;

    public IncidentController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    public IActionResult Index(string? search, string? type)
    {
        ViewBag.ActiveMenu = "Incident";

        var receipts = _readerService.GetAllFinancialReceipts(search, type);
        var summary = _readerService.GetFinancialSummary();

        var model = new IncidentIndexViewModel
        {
            TongThu = summary.TongThu,
            ThuNopMuon = summary.ThuNopMuon,
            ThuDenBu = summary.ThuDenBu,
            SearchKeyword = search,
            SelectedType = type ?? "Tất cả",
            Receipts = receipts
        };

        return View(model);
    }
}
