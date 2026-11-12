using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN,QUANLY_TAILIEU,Thuthu")]
public class CatalogController : AdminBaseController
{
    private readonly IBookCatalogService _catalogService;

    public CatalogController(IBookCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public IActionResult Index(
        string? search, 
        string? category, 
        string? major, 
        string? language, 
        string? status)
    {
        ViewBag.ActiveMenu = "Catalog";

        var cat = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim();
        var maj = string.IsNullOrWhiteSpace(major) ? "all" : major.Trim();
        var lang = string.IsNullOrWhiteSpace(language) ? "all" : language.Trim();
        var avail = string.IsNullOrWhiteSpace(status) ? "all" : (status == "Có sẵn" ? "available" : status.Trim());
        var kw = search?.Trim() ?? string.Empty;

        var items = _catalogService.Search(kw, cat, maj, lang, avail).ToList();
        var allBooks = _catalogService.GetAll().ToList();

        var model = new CatalogIndexViewModel
        {
            SearchKeyword = kw,
            SelectedCategory = cat,
            SelectedMajor = maj,
            SelectedLanguage = lang,
            SelectedStatus = avail,
            TotalCatalogBooks = allBooks.Count,
            TotalCatalogCopies = allBooks.Sum(b => b.TotalCopies),
            TotalCatalogAvailable = allBooks.Sum(b => b.AvailableCopies),
            TotalCatalogBorrowed = allBooks.Sum(b => Math.Max(0, b.TotalCopies - b.AvailableCopies)),
            Books = items.Select(b => new CatalogItemViewModel
            {
                Id = b.Id,
                MaHocLieu = $"TLVN26-{b.Id:D3}",
                TenHocLieu = b.Title,
                TacGia = b.Authors,
                NhaXuatBan = b.Publisher,
                ISBN = b.ISBN,
                NamXuatBan = b.Year,
                SoTrang = b.Pages,
                TheLoai = b.MajorName,
                MajorCode = b.Major,
                DinhDang = b.IsOnline ? "Bản điện tử" : "Sách in",
                CoverImage = string.IsNullOrWhiteSpace(b.CoverImage) ? "/images/covers/sach_01.jpg" : b.CoverImage,
                TongBan = b.TotalCopies,
                SanCo = b.AvailableCopies,
                DangMuon = Math.Max(0, b.TotalCopies - b.AvailableCopies),
                ViTriKe = b.Shelf,
                MoTa = b.Description,
                Copies = b.Copies ?? new List<BookCopyItem>()
            }).ToList()
        };

        return View(model);
    }

    // THÊM HỌC LIỆU MỚI VÀO CSDL
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(
        string title, 
        string authors, 
        string publisher, 
        string major, 
        int year, 
        int pages, 
        string isbn, 
        string shelf, 
        int copiesCount, 
        string? coverImage, 
        string? description)
    {
        string majorName = major switch
        {
            "cntt" => "Công nghệ thông tin",
            "xaydung" => "Kỹ thuật xây dựng & Giao thông",
            "kinhte" => "Kinh tế & Quản trị",
            "toanly" => "Khoa học cơ bản",
            "chinhtri" => "Lý luận chính trị & Pháp luật",
            _ => "Công nghệ thông tin"
        };

        var book = new BookItem
        {
            Title = title?.Trim() ?? string.Empty,
            Authors = authors?.Trim() ?? string.Empty,
            Publisher = string.IsNullOrWhiteSpace(publisher) ? "NXB Đại học Giao thông Vận tải" : publisher.Trim(),
            Major = major ?? "cntt",
            MajorName = majorName,
            Category = "books",
            CategoryName = "Sách & Giáo trình",
            Year = year > 0 ? year : DateTime.Today.Year,
            Pages = pages > 0 ? pages : 250,
            ISBN = isbn?.Trim() ?? string.Empty,
            Shelf = string.IsNullOrWhiteSpace(shelf) ? "Kệ IT-01 - Tầng 2 A8" : shelf.Trim(),
            CoverImage = string.IsNullOrWhiteSpace(coverImage) ? "/images/covers/sach_01.jpg" : coverImage.Trim(),
            Description = description?.Trim() ?? string.Empty,
            IsOnline = false
        };

        var success = _catalogService.AddBook(book, copiesCount > 0 ? copiesCount : 4, out int newId, out string errorMessage);
        if (success)
        {
            TempData["SuccessMessage"] = $"Đã thêm thành công học liệu '{book.Title}' (Mã: TLVN26-{newId:D3}) với {Math.Max(1, copiesCount)} bản sao vào CSDL Thư viện UTC!";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // CHỈNH SỬA THÔNG TIN HỌC LIỆU TRONG CSDL
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(
        int id, 
        string title, 
        string authors, 
        string publisher, 
        string major, 
        int year, 
        int pages, 
        string isbn, 
        string shelf, 
        string? coverImage, 
        string? description)
    {
        string majorName = major switch
        {
            "cntt" => "Công nghệ thông tin",
            "xaydung" => "Kỹ thuật xây dựng & Giao thông",
            "kinhte" => "Kinh tế & Quản trị",
            "toanly" => "Khoa học cơ bản",
            "chinhtri" => "Lý luận chính trị & Pháp luật",
            _ => "Công nghệ thông tin"
        };

        var book = new BookItem
        {
            Id = id,
            Title = title?.Trim() ?? string.Empty,
            Authors = authors?.Trim() ?? string.Empty,
            Publisher = publisher?.Trim() ?? string.Empty,
            Major = major ?? "cntt",
            MajorName = majorName,
            Year = year,
            Pages = pages,
            ISBN = isbn?.Trim() ?? string.Empty,
            Shelf = shelf?.Trim() ?? "Kệ IT-01",
            CoverImage = coverImage?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? string.Empty
        };

        var success = _catalogService.UpdateBook(book, out string errorMessage);
        if (success)
        {
            TempData["SuccessMessage"] = $"Đã cập nhật thông tin học liệu '{book.Title}' trong CSDL thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // XÓA HỌC LIỆU KHỎI CSDL
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var success = _catalogService.DeleteBook(id, out string errorMessage);
        if (success)
        {
            TempData["SuccessMessage"] = "Đã xóa học liệu khỏi CSDL Thư viện UTC thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // API LẤY CHI TIẾT BẢN SAO CHO MODAL AJAX
    [HttpGet]
    [Route("/Admin/Catalog/GetCopies/{id?}")]
    [Route("/Admin/Catalog/GetCopies")]
    public IActionResult GetCopies(int? id)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            return BadRequest(new { message = "Mã học liệu không hợp lệ!" });
        }

        var book = _catalogService.GetById(id.Value);
        if (book == null)
        {
            return NotFound(new { message = "Không tìm thấy học liệu!" });
        }

        return Json(new
        {
            bookId = book.Id,
            title = book.Title,
            coverImage = book.CoverImage,
            shelf = book.Shelf,
            totalCopies = book.TotalCopies,
            availableCopies = book.AvailableCopies,
            copies = book.Copies
        });
    }

    // API LẤY THÔNG TIN HỌC LIỆU ĐỂ ĐIỀN VÀO MODAL SỬA
    [HttpGet]
    [Route("/Admin/Catalog/GetBook/{id?}")]
    [Route("/Admin/Catalog/GetBook")]
    public IActionResult GetBook(int? id)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            return BadRequest(new { message = "Mã học liệu không hợp lệ!" });
        }

        var book = _catalogService.GetById(id.Value);
        if (book == null)
        {
            return NotFound(new { message = "Không tìm thấy học liệu!" });
        }

        return Json(new
        {
            id = book.Id,
            title = book.Title,
            authors = book.Authors,
            publisher = book.Publisher,
            major = book.Major,
            majorName = book.MajorName,
            year = book.Year,
            pages = book.Pages,
            isbn = book.ISBN,
            shelf = book.Shelf,
            coverImage = book.CoverImage,
            description = book.Description
        });
    }
}
