using System.Security.Claims;
﻿using Microsoft.AspNetCore.Mvc;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN,QUANLY_BANDOC")]
public class ReaderController : AdminBaseController
{
    private readonly IReaderService _readerService;

    public ReaderController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    // Figma 03: Quản lý bạn đọc
    [HttpGet]
    public IActionResult Index(string? search, string? group, int borrowingPage = 1, int nonBorrowingPage = 1)
    {
        ViewBag.ActiveMenu = "Reader";
        var model = _readerService.GetReaders(search, group);
        model.BorrowingPagination = new AdminPaginationViewModel
        {
            TotalItems = model.BorrowingReaders.Count, RequestedPage = borrowingPage,
            Controller = "Reader", Fragment = "borrowingReaders", Unit = "bạn đọc",
            PageParameter = "borrowingPage", AriaLabel = "Trang bạn đọc đang mượn"
        };
        model.NonBorrowingPagination = new AdminPaginationViewModel
        {
            TotalItems = model.NonBorrowingReaders.Count, RequestedPage = nonBorrowingPage,
            Controller = "Reader", Fragment = "nonBorrowingReaders", Unit = "bạn đọc",
            PageParameter = "nonBorrowingPage", AriaLabel = "Trang bạn đọc chưa mượn"
        };
        model.BorrowingPagination.Filters = new()
        {
            ["search"] = model.SearchKeyword, ["group"] = model.SelectedGroup,
            ["nonBorrowingPage"] = model.NonBorrowingPagination.CurrentPage.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        model.NonBorrowingPagination.Filters = new()
        {
            ["search"] = model.SearchKeyword, ["group"] = model.SelectedGroup,
            ["borrowingPage"] = model.BorrowingPagination.CurrentPage.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
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

        var success = _readerService.BorrowBook(maBD, bookId, out string bookTitle, out string errorMessage, User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (success)
        {
            string dueDateStr = BTL_LTWEB.Common.LoanTermPolicy.TryGetDueDate(DateOnly.FromDateTime(DateTime.Today), out var dueDate) ? dueDate.ToString("dd/MM/yyyy") : "theo phiếu mượn";
            TempData["SuccessMessage"] = $"Xác nhận cho mượn thành công cuốn sách '{bookTitle}'! Hạn trả cuối kỳ: {dueDateStr}.";
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
        if (_readerService.ProcessImport(out int updated, out int inserted))
            TempData["SuccessMessage"] = $"Đã cập nhật {updated} và thêm {inserted} bạn đọc.";
        else
            TempData["ErrorMessage"] = "Chức năng nhập QLĐT chưa được cấu hình; chưa có dữ liệu nào được đồng bộ.";
        return RedirectToAction(nameof(Index));
    }
}
