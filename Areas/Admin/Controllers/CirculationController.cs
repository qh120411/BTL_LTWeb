using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN,QUANLY_BANDOC")]
public class CirculationController : AdminBaseController
{
    private readonly IReaderService _readerService;
    private readonly IBookCatalogService _catalogService;

    public CirculationController(IReaderService readerService, IBookCatalogService catalogService)
    {
        _readerService = readerService;
        _catalogService = catalogService;
    }

    public IActionResult Index(string? search, string? status)
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

        var readers = _readerService.GetReaders(null, null).Readers;
        var books = _catalogService.GetAll().ToList();
        var borrowedMap = _readerService.GetReaderBorrowedTitlesMap();

        var model = new CirculationIndexViewModel
        {
            SearchKeyword = search,
            SelectedStatus = status ?? "Tất cả trạng thái",
            Transactions = loans,
            AvailableReaders = readers,
            AvailableBooks = books,
            ReaderBorrowedTitlesMap = borrowedMap
        };

        return View(model);
    }

    // Xử lý tạo giao dịch mượn tài liệu học liệu mới tại quầy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateBorrow(string maBD, int bookId)
    {
        if (string.IsNullOrWhiteSpace(maBD) || bookId <= 0)
        {
            TempData["ErrorMessage"] = "Thông tin tạo giao dịch mượn không hợp lệ! Vui lòng chọn bạn đọc và học liệu.";
            return RedirectToAction(nameof(Index));
        }

        var success = _readerService.BorrowBook(maBD, bookId, out string bookTitle, out string errorMessage);
        if (success)
        {
            string dueDateStr = DateTime.Today.AddYears(1).ToString("dd/MM/yyyy");
            TempData["SuccessMessage"] = $"Tạo giao dịch mượn thành công! Bạn đọc mã '{maBD}' đã mượn cuốn sách '{bookTitle}'. Thời hạn mượn luôn là 1 năm (Hạn trả: {dueDateStr}). CSDL đã được cập nhật.";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // Xử lý thu hồi / trả sách ngay tại quầy lưu thông
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReturnBook(string maBD, string maGD)
    {
        if (string.IsNullOrWhiteSpace(maBD) || string.IsNullOrWhiteSpace(maGD))
        {
            TempData["ErrorMessage"] = "Thông tin thu hồi trả sách không hợp lệ!";
            return RedirectToAction(nameof(Index));
        }

        var success = _readerService.ReturnBook(maBD, maGD, out string bookTitle, out decimal reducedFine);
        if (success)
        {
            string fineMsg = reducedFine > 0 
                ? $" Đã giải phóng {reducedFine:N0} VNĐ tiền phạt quá hạn (Tổng cần thanh toán: 0 VNĐ)." 
                : "";
            TempData["SuccessMessage"] = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{maBD}' thành công!{fineMsg} CSDL đã được cập nhật.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không tìm thấy giao dịch hoặc sách đã được hoàn trả trước đó!";
        }

        return RedirectToAction(nameof(Index));
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

        actionType = string.IsNullOrWhiteSpace(actionType) ? "NormalReturn" : actionType;
        compensationType = string.IsNullOrWhiteSpace(compensationType) ? "NewBook" : compensationType;
        decimal compAmount = compensationAmount ?? 150000m;

        var success = _readerService.ProcessReturnOrCompensation(
            maBD, 
            maGD, 
            actionType, 
            compensationType, 
            compAmount, 
            payOverdue, 
            note, 
            out string message);

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
