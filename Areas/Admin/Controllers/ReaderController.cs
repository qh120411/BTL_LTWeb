using Microsoft.AspNetCore.Mvc;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN,QUANLY_BANDOC,THUTHU")]
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
        TempData["ErrorMessage"] = "Dùng form nhập TSV; chưa có kết nối QLĐT tự động.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ImportTsv(string rows, [FromServices] BTL_LTWEB.Models.Entities.QLThuVienUtcContext db) {
        if(string.IsNullOrWhiteSpace(rows)||rows.Length>100000){TempData["ErrorMessage"]="Danh sách trống hoặc quá lớn.";return RedirectToAction("ImportQldt");}
        var lines=rows.Replace("\r","").Split('\n',StringSplitOptions.RemoveEmptyEntries);
        var parsed=lines.Select(l=>l.Split('\t')).ToList();
        var ids=new HashSet<string>();var emails=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if(lines.Length>200||parsed.Any(c=>c.Length!=3||c[0].Trim().Length<5||c[0].Trim().Length>30||!c[0].Trim().All(char.IsDigit)||c[1].Trim().Length<2||c[1].Length>150||!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(c[2].Trim())||c[2].Length>150||!ids.Add(c[0].Trim())||!emails.Add(c[2].Trim()))) {
            TempData["ErrorMessage"]="Danh sách có dòng sai định dạng hoặc trùng mã/email. Chưa nhập dòng nào.";return RedirectToAction("ImportQldt");
        }
        using var tx=db.Database.BeginTransaction();
        if(db.BanDocs.Any(b=>ids.Contains(b.MaSinhVien)||(b.Email!=null&&emails.Contains(b.Email)))) {TempData["ErrorMessage"]="Mã sinh viên hoặc email đã tồn tại. Chưa nhập dòng nào.";return RedirectToAction("ImportQldt");}
        foreach(var c in parsed)db.BanDocs.Add(new BTL_LTWEB.Models.Entities.BanDoc{MaSinhVien=c[0].Trim(),HoTen=c[1].Trim(),Email=c[2].Trim(),TrangThai="Đang học",NgayTao=DateTime.Now});
        db.SaveChanges();tx.Commit();TempData["SuccessMessage"]=$"Đã nhập {parsed.Count} bạn đọc. Chưa cấp tài khoản hoặc thẻ.";return RedirectToAction("Index");
    }
}
