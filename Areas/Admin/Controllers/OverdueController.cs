using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN,QUANLY_BANDOC")]
public class OverdueController : AdminBaseController
{
    private readonly IReaderService _readerService;

    public OverdueController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    public IActionResult Index(string? search, int page = 1)
    {
        ViewBag.ActiveMenu = "Overdue";

        var overdues = _readerService.GetAllOverdueLoans();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim().ToLower();
            overdues = overdues.Where(x => x.MaBD.ToLower().Contains(kw) ||
                                           x.HoTen.ToLower().Contains(kw) ||
                                           x.TenSach.ToLower().Contains(kw)).ToList();
        }

        var model = new OverdueIndexViewModel
        {
            SearchKeyword = search,
            OverdueList = overdues,
            Pagination = new AdminPaginationViewModel
            {
                TotalItems = overdues.Count, RequestedPage = page,
                Controller = "Overdue", Fragment = "overdueList", Unit = "lượt quá hạn",
                Filters = new() { ["search"] = search }
            }
        };

        return View(model);
    }
}
