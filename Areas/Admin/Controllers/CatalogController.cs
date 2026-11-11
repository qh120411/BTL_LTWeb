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

    public IActionResult Index(string? search, string? major, string? status, string? category = null, string? language = null, int page = 1)
    {
        ViewBag.ActiveMenu = "Catalog";

        search = search?.Trim();
        if (search?.Length > 100) search = search[..100];
        category = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim();
        language = string.IsNullOrWhiteSpace(language) ? "all" : language.Trim();
        var books = _catalogService.GetAll();
        if (category != "all") books = books.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (language != "all") books = books.Where(b => b.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim().ToLower();
            books = books.Where(b => b.Title.ToLower().Contains(kw) ||
                                     b.Authors.ToLower().Contains(kw) ||
                                     b.Publisher.ToLower().Contains(kw) ||
                                     b.ISBN.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(major) && major != "all" && major != "Tất cả chuyên ngành")
        {
            var m = major.Trim().ToLower();
            books = books.Where(b => 
                b.Major.Equals(major, StringComparison.OrdinalIgnoreCase) ||
                b.MajorName.Equals(major, StringComparison.OrdinalIgnoreCase) ||
                (m.Contains("thông tin") && b.Major == "cntt") ||
                ((m.Contains("xây dựng") || m.Contains("công trình") || m.Contains("giao thông")) && b.Major == "xaydung") ||
                ((m.Contains("kinh tế") || m.Contains("quản trị") || m.Contains("logistics")) && b.Major == "kinhte") ||
                ((m.Contains("khoa học") || m.Contains("cơ bản") || m.Contains("toán")) && b.Major == "toanly") ||
                ((m.Contains("chính trị") || m.Contains("pháp luật")) && b.Major == "chinhtri")
            );
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả")
        {
            if (status == "Có sẵn")
            {
                books = books.Where(b => !b.IsOnline && b.AvailableCopies > 0);
            }
            else if (status == "Hết sách")
            {
                books = books.Where(b => !b.IsOnline && b.AvailableCopies <= 0);
            }
            else if (status == "Bản điện tử") books = books.Where(b => b.IsOnline);
        }

        var model = new CatalogIndexViewModel
        {
            SearchKeyword = search,
            SelectedCategory = category,
            SelectedLanguage = language,
            SelectedMajor = major ?? "Tất cả chuyên ngành",
            SelectedStatus = status ?? "Tất cả",
            Books = books.Select(b => new CatalogItemViewModel
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
                IsOnline = b.IsOnline,
                CoverImage = string.IsNullOrWhiteSpace(b.CoverImage) ? "/images/covers/sach_01.jpg" : b.CoverImage,
                TongBan = b.TotalCopies,
                SanCo = b.AvailableCopies,
                DangMuon = Math.Max(0, b.TotalCopies - b.AvailableCopies),
                ViTriKe = b.Shelf,
                MoTa = b.Description,
                Copies = b.Copies ?? new List<BookCopyItem>()
            }).ToList()
        };

        model.TrangHienTai = Math.Clamp(page, 1, model.TongTrang);

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
            TempData["SuccessMessage"] = $"Đã thêm học liệu: {book.Title}.";
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
            TempData["SuccessMessage"] = "Đã cập nhật học liệu.";
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
            TempData["SuccessMessage"] = "Đã xóa học liệu.";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // API LẤY CHI TIẾT BẢN SAO CHO MODAL AJAX
    [HttpGet]
    public IActionResult GetCopies(int id)
    {
        var book = _catalogService.GetById(id);
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
    public IActionResult GetBook(int id)
    {
        var book = _catalogService.GetById(id);
        if (book == null)
        {
            return NotFound();
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
