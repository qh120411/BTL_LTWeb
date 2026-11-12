using System.Security.Claims;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN,Admin,QUANLY_BANDOC,QUANLY_TAILIEU,Thuthu,THUTHU,AdminStaff")]
public class CirculationController : AdminBaseController
{
    private readonly IReaderService _readerService;
    private readonly IBookCatalogService _catalogService;
    private readonly QLThuVienUtcContext _dbContext;

    public CirculationController(
        IReaderService readerService,
        IBookCatalogService catalogService,
        QLThuVienUtcContext dbContext)
    {
        _readerService = readerService;
        _catalogService = catalogService;
        _dbContext = dbContext;
    }

    public IActionResult Index(string? search, string? status, int page = 1)
    {
        ViewBag.ActiveMenu = "Circulation";

        var loans = _readerService.GetAllCirculationTransactions();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim().ToLower();
            loans = loans.Where(x => x.MaGD.ToLower().Contains(kw) ||
                                     x.MaBD.ToLower().Contains(kw) ||
                                     x.HoTen.ToLower().Contains(kw) ||
                                     x.TenSach.ToLower().Contains(kw)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả trạng thái")
        {
            if (status == "Quá hạn")
            {
                loans = loans.Where(x => x.IsQuaHan).ToList();
            }
            else if (status == "Đang mượn")
            {
                loans = loans.Where(x => !x.IsQuaHan).ToList();
            }
        }

        var books = _catalogService.GetAll().Where(b => !b.IsOnline).ToList();

        // Đồng bộ số bản sao sẵn có chính xác từ SQL Server

        var model = new CirculationIndexViewModel
        {
            SearchKeyword = search,
            SelectedStatus = status ?? "Tất cả trạng thái",
            Transactions = loans,
            AvailableBooks = books,
            Pagination = new AdminPaginationViewModel
            {
                TotalItems = loans.Count,
                RequestedPage = page,
                Controller = "Circulation",
                Fragment = "circulationList",
                Unit = "giao dịch",
                Filters = new() { ["search"] = search, ["status"] = status }
            }
        };

        return View(model);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> SearchReaders(string? query, CancellationToken cancellationToken)
    {
        return Json(new { readers = await _readerService.SearchBorrowReadersAsync(query, cancellationToken) });
    }

    // Xử lý tạo giao dịch mượn tài liệu học liệu mới tại quầy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateBorrow(string maBD, int bookId, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(maBD) || bookId <= 0)
        {
            TempData["ErrorMessage"] = "Thông tin tạo giao dịch mượn không hợp lệ! Vui lòng chọn bạn đọc và học liệu.";
            return RedirectToLocal(returnUrl);
        }

        var success = _readerService.BorrowBook(maBD, bookId, out string bookTitle, out string errorMessage, User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (success)
        {
            string dueDateStr = BTL_LTWEB.Common.LoanTermPolicy.TryGetDueDate(DateOnly.FromDateTime(DateTime.Today), out var dueDate) ? dueDate.ToString("dd/MM/yyyy") : "theo phiếu mượn";
            TempData["SuccessMessage"] = $"Tạo giao dịch mượn thành công! Bạn đọc mã '{maBD}' đã mượn cuốn sách '{bookTitle}'. Hạn trả cuối kỳ: {dueDateStr}.";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToLocal(returnUrl);
    }

    // Xử lý xác nhận trả sách hoặc bồi thường (mất/hỏng) từ Modal dùng chung
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessReturnOrCompensation(
        string maBD,
        string maGD,
        string actionType,
        string? compensationType,
        decimal? compensationAmount,
        bool payOverdue,
        string? note,
        string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(maBD) || string.IsNullOrWhiteSpace(maGD))
        {
            TempData["ErrorMessage"] = "Thông tin xử lý trả sách / bồi thường không hợp lệ!";
            return RedirectToLocal(returnUrl);
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (actionType != "NormalReturn" && actionType != "Compensation")
            return BadRequest("Vui lòng chọn hình thức xử lý hợp lệ.");
        if (actionType == "Compensation" && compensationType != "Cash" && compensationType != "NewBook")
            return BadRequest("Vui lòng chọn hình thức bồi thường.");
        decimal compAmount = 0;
        if (actionType == "Compensation" && compensationType == "Cash")
        {
            if (!compensationAmount.HasValue || compensationAmount <= 0 || compensationAmount > 100000000m)
                return BadRequest("Vui lòng nhập số tiền bồi thường hợp lệ.");
            compAmount = compensationAmount.Value;
        }

        var success = _readerService.ProcessReturnOrCompensation(
            maBD,
            maGD,
            actionType,
            compensationType!,
            compAmount,
            payOverdue,
            note,
            out string message, User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToLocal(returnUrl);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }
}
