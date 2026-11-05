using Microsoft.AspNetCore.Mvc;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class ReaderController : AdminBaseController
{
    private readonly IReaderService _readerService;

    public ReaderController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    // Figma 03: Quản lý bạn đọc
    [HttpGet]
    public IActionResult Index(string? search, string? group)
    {
        ViewBag.ActiveMenu = "Reader";
        var model = _readerService.GetReaders(search, group);
        return View(model);
    }

    // Figma 11: Hồ sơ bạn đọc (kèm danh sách sách đang mượn sổ ra trực tiếp)
    [HttpGet]
    public IActionResult Details(string id)
    {
        ViewBag.ActiveMenu = "Reader";

        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var model = _readerService.GetReaderById(id);
        if (model == null)
        {
            TempData["ErrorMessage"] = $"Không tìm thấy bạn đọc với mã '{id}'";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // Figma 11: Lưu thiết lập thư viện
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateProfile(ReaderDetailViewModel model)
    {
        ViewBag.ActiveMenu = "Reader";

        var result = _readerService.UpdateReaderSettings(model);
        if (result)
        {
            TempData["SuccessMessage"] = "Đã lưu thiết lập thư viện thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu thiết lập bạn đọc.";
        }

        return RedirectToAction(nameof(Details), new { id = model.MaBD });
    }

    // Trả sách cho bạn đọc
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReturnBook(string maBD, string maGD)
    {
        if (string.IsNullOrWhiteSpace(maBD) || string.IsNullOrWhiteSpace(maGD))
        {
            TempData["ErrorMessage"] = "Thông tin trả sách không hợp lệ!";
            return RedirectToAction(nameof(Index));
        }

        var success = _readerService.ReturnBook(maBD, maGD, out string bookTitle, out decimal reducedFine);
        if (success)
        {
            string fineMsg = reducedFine > 0 ? " Toàn bộ tiền phạt quá hạn của sách đã được tất toán (0 VNĐ)." : "";
            TempData["SuccessMessage"] = $"Thủ thư đã tiếp nhận và hoàn tất trả cuốn sách '{bookTitle}' thành công!{fineMsg}";
        }
        else
        {
            TempData["ErrorMessage"] = "Không tìm thấy giao dịch mượn hoặc sách đã được trả trước đó!";
        }

        return RedirectToAction(nameof(Details), new { id = maBD });
    }

    // Mượn thêm sách theo phong cách tra cứu OPAC
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BorrowBook(string maBD, int bookId)
    {
        if (string.IsNullOrWhiteSpace(maBD) || bookId <= 0)
        {
            TempData["ErrorMessage"] = "Thông tin mượn sách không hợp lệ!";
            return RedirectToAction(nameof(Index));
        }

        var success = _readerService.BorrowBook(maBD, bookId, out string bookTitle, out string errorMessage);
        if (success)
        {
            string dueDateStr = DateTime.Today.AddYears(1).ToString("dd/MM/yyyy");
            TempData["SuccessMessage"] = $"Xác nhận cho mượn thành công cuốn sách '{bookTitle}'! Thời hạn mượn luôn là 1 năm (Hạn trả: {dueDateStr}). CSDL đã được cập nhật.";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToAction(nameof(Details), new { id = maBD });
    }

    // Nhập dữ liệu từ phòng Đào tạo (QLĐT)
    [HttpGet]
    public IActionResult ImportQldt()
    {
        ViewBag.ActiveMenu = "Reader";
        var model = _readerService.GetImportPreview();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessImport()
    {
        _readerService.ProcessImport(out int updated, out int inserted);
        TempData["SuccessMessage"] = $"Đồng bộ thành công {updated} sinh viên từ phòng Đào tạo (QLĐT)!";
        return RedirectToAction(nameof(Index));
    }
}
