using Microsoft.AspNetCore.Mvc;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
public class ReaderController : Controller
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

    // Figma 11: Hồ sơ bạn đọc
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

        if (!ModelState.IsValid)
        {
            var current = _readerService.GetReaderById(model.MaBD);
            if (current != null)
            {
                model.BorrowTransactions = current.BorrowTransactions;
            }
            return View("Details", model);
        }

        var result = _readerService.UpdateReaderSettings(model);
        if (result)
        {
            TempData["SuccessMessage"] = "Đã lưu thiết lập thư viện thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Cập nhật thiết lập thư viện thất bại.";
        }

        return RedirectToAction(nameof(Details), new { id = model.MaBD });
    }

    // Lấy chi tiết lịch sử mượn (cho modal)
    [HttpGet]
    public IActionResult GetBorrowHistory(string id)
    {
        var model = _readerService.GetReaderById(id);
        if (model == null) return NotFound();
        return Json(model.BorrowTransactions);
    }

    // Figma 12: Nhập dữ liệu QLĐT
    [HttpGet]
    public IActionResult ImportQldt()
    {
        ViewBag.ActiveMenu = "Reader";
        var model = _readerService.GetImportPreview();
        return View(model);
    }

    // Figma 12: Xác nhận nhập dữ liệu QLĐT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmImportQldt()
    {
        _readerService.ProcessImport(out int updated, out int inserted);
        TempData["SuccessMessage"] = $"Nhập dữ liệu thành công! Đã cập nhật {updated} sinh viên và thêm mới {inserted} sinh viên vào hệ thống.";
        return RedirectToAction(nameof(Index));
    }
}

