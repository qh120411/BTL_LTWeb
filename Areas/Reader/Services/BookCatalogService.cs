using System.Globalization;
using System.Text;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Reader.Services;
public interface IBookCatalogService
{
    IEnumerable<BookItem> GetAll();
    BookItem? GetById(int id);
    IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability);
    bool AddBook(BookItem book, int copiesCount, out int newId, out string errorMessage);
    bool UpdateBook(BookItem book, out string errorMessage);
    bool DeleteBook(int id, out string errorMessage);
    List<BookCopyItem> GetCopiesByBookId(int bookId);
}

// SQL Server / SQLite dùng cùng nguồn TaiLieu và BanSao; không ghi catalog_books.json.
public class BookCatalogService(QLThuVienUtcContext db, ILogger<BookCatalogService> logger) : IBookCatalogService
{
    private IQueryable<TaiLieu> Query() => db.TaiLieus.AsNoTracking().Include(t => t.BanSaos).Include(t => t.TaiLieuTacGia).ThenInclude(t => t.MaTacGiaNavigation).Include(t => t.MaNhaXuatBanNavigation).Include(t => t.MaTheLoaiNavigation).Include(t => t.MaNgonNguNavigation).Include(t => t.MaDinhDangNavigation);
    public IEnumerable<BookItem> GetAll() => Query().OrderBy(t => t.MaTaiLieu).ToList().Select(Map).ToList();
    public BookItem? GetById(int id)
    {
        var t = Query().FirstOrDefault(t => t.MaTaiLieu == id);
        return t == null ? null : Map(t);
    }

    public static string Normalize(string? s) => new string ((s ?? "").Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
    public IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability)
    {
        var kw = Normalize(keyword);
        return GetAll().Where(b => (kw.Length == 0 || Normalize($"{b.Title} {b.Authors} {b.ISBN} {b.Publisher} {b.MajorName}").Contains(kw)) && (string.IsNullOrEmpty(category) || category == "all" || category == b.Category) && (string.IsNullOrEmpty(major) || major == "all" || major == b.Major) && (string.IsNullOrEmpty(language) || language == "all" || language == b.Language) && (string.IsNullOrEmpty(availability) || availability == "all" || (availability == "available" && b.AvailableCopies > 0) || (availability == "unavailable" && b.AvailableCopies == 0) || (availability == "online" && b.IsOnline))).ToList();
    }

    private static BookItem Map(TaiLieu t)
    {
        var genre = t.MaTheLoaiNavigation?.TenTheLoai ?? "Chưa phân loại";
        var norm = Normalize(genre);
        var major = norm.Contains("cong nghe thong tin") ? "cntt" : norm.Contains("kinh te") || norm.Contains("logistics") ? "kinhte" : norm.Contains("chinh tri") ? "chinhtri" : norm.Contains("toan") ? "toanly" : "xaydung";
        var format = Normalize(t.MaDinhDangNavigation?.TenDinhDang);
        return new BookItem
        {
            Id = t.MaTaiLieu,
            Title = t.TenTaiLieu,
            Authors = string.Join(", ", t.TaiLieuTacGia.Select(x => x.MaTacGiaNavigation.HoTen)),
            Publisher = t.MaNhaXuatBanNavigation?.TenNhaXuatBan ?? "",
            Category = norm.Contains("luan") ? "thesis" : norm.Contains("tap chi") || norm.Contains("bao ") ? "journal" : "books",
            CategoryName = genre,
            Major = major,
            MajorName = genre,
            Language = t.MaNgonNgu?.Trim() == "ENG" ? "eng" : "vie",
            LanguageName = t.MaNgonNguNavigation?.TenNgonNgu ?? "",
            Year = t.NamXuatBan ?? 0,
            Pages = t.SoTrang ?? 0,
            ISBN = t.Isbn ?? "",
            CoverImage = SafeImage(t.AnhBia),
            Description = t.MoTa ?? "",
            IsOnline = format.Contains("dien tu"),
            Shelf = t.BanSaos.FirstOrDefault()?.ViTriKe ?? "Liên hệ quầy",
            TotalCopies = t.BanSaos.Count,
            AvailableCopies = t.BanSaos.Count(b => b.TrangThai == "Có sẵn"),
            Copies = t.BanSaos.Select(Copy).ToList()
        };
    }

    private static string SafeImage(string? path) => !string.IsNullOrEmpty(path) && path.StartsWith("/images/", StringComparison.Ordinal) && !path.Contains("..") ? path : "/utc/assets/images/logo.png";
    private static BookCopyItem Copy(BanSao b) => new()
    {
        Id = b.MaBanSao,
        BookId = b.MaTaiLieu,
        Barcode = b.MaVach,
        Shelf = b.ViTriKe ?? "",
        Status = b.TrangThai,
        Condition = b.TinhTrang,
        ImportDate = b.NgayNhap?.ToString("dd/MM/yyyy") ?? ""
    };
    public List<BookCopyItem> GetCopiesByBookId(int id) => db.BanSaos.AsNoTracking().Where(b => b.MaTaiLieu == id).OrderBy(b => b.MaBanSao).ToList().Select(Copy).ToList();
    private void Apply(TaiLieu t, BookItem b)
    {
        t.TenTaiLieu = b.Title.Trim();
        t.Isbn = string.IsNullOrWhiteSpace(b.ISBN) ? null : b.ISBN.Trim();
        t.NamXuatBan = (short)b.Year;
        t.SoTrang = b.Pages;
        t.MoTa = b.Description;
        t.AnhBia = SafeImage(b.CoverImage);
        t.MaNgonNgu ??= "VIE";
        var genre = db.TheLoais.FirstOrDefault(x => x.TenTheLoai == b.MajorName) ?? new TheLoai
        {
            TenTheLoai = b.MajorName
        };
        t.MaTheLoaiNavigation = genre;
        var publisher = db.NhaXuatBans.FirstOrDefault(x => x.TenNhaXuatBan == b.Publisher) ?? new NhaXuatBan
        {
            TenNhaXuatBan = b.Publisher
        };
        t.MaNhaXuatBanNavigation = publisher;
        var names = b.Authors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var link in t.TaiLieuTacGia.ToList())
        {
            if (!names.Contains(link.MaTacGiaNavigation.HoTen))
            {
                t.TaiLieuTacGia.Remove(link);
                db.TaiLieuTacGia.Remove(link);
            }
        }
        foreach (var name in names)
        {
            if (t.TaiLieuTacGia.Any(link => string.Equals(link.MaTacGiaNavigation.HoTen, name, StringComparison.OrdinalIgnoreCase))) continue;
            var author = db.TacGia.FirstOrDefault(x => x.HoTen == name) ?? new TacGium { HoTen = name };
            t.TaiLieuTacGia.Add(new TaiLieuTacGium { MaTacGiaNavigation = author, VaiTroTacGia = "Tác giả" });
        }
    }

    private static string? Validate(BookItem b) => string.IsNullOrWhiteSpace(b.Title) || string.IsNullOrWhiteSpace(b.Authors) ? "Nhập tên tài liệu và tác giả." : b.Title.Length > 500 || b.Authors.Length > 500 ? "Tên tài liệu hoặc tác giả quá dài." : b.Year < 1000 || b.Year > DateTime.Today.Year + 1 || b.Pages < 1 ? "Kiểm tra năm xuất bản và số trang." : null;
    public bool AddBook(BookItem b, int count, out int newId, out string error)
    {
        newId = 0;
        error = Validate(b) ?? "";
        if (error.Length > 0)
            return false;
        if (count < 1 || count > 200)
        {
            error = "Số bản từ 1 đến 200.";
            return false;
        }

        if (db.TaiLieus.Any(t => t.TenTaiLieu == b.Title.Trim()))
        {
            error = "Tên học liệu đã tồn tại.";
            return false;
        }

        using var tx = db.Database.BeginTransaction();
        try
        {
            var t = new TaiLieu
            {
                NgayTao = DateTime.Now
            };
            Apply(t, b);
            db.TaiLieus.Add(t);
            db.SaveChanges();
            newId = t.MaTaiLieu;
            for (var i = 1; i <= count; i++)
                db.BanSaos.Add(new BanSao { MaTaiLieu = newId, MaVach = $"UTC-{newId:D5}-{i:D3}", TrangThai = "Có sẵn", TinhTrang = "Tốt", ViTriKe = b.Shelf, NgayNhap = DateOnly.FromDateTime(DateTime.Today) });
            db.SaveChanges();
            tx.Commit();
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Không lưu được học liệu");
            error = "Không lưu được học liệu. Kiểm tra ISBN và danh mục dữ liệu.";
            return false;
        }
    }

    public bool UpdateBook(BookItem b, out string error)
    {
        error = Validate(b) ?? "";
        if (error.Length > 0)
            return false;
        var t = db.TaiLieus.Include(t => t.TaiLieuTacGia).ThenInclude(t => t.MaTacGiaNavigation).FirstOrDefault(t => t.MaTaiLieu == b.Id);
        if (t == null)
        {
            error = "Không tìm thấy tài liệu.";
            return false;
        }

        try
        {
            Apply(t, b);
            db.SaveChanges();
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Không cập nhật được học liệu");
            error = "Không cập nhật được học liệu. Kiểm tra ISBN.";
            return false;
        }
    }

    public bool DeleteBook(int id, out string error)
    {
        error = "";
        var t = db.TaiLieus.Include(t => t.BanSaos).Include(t => t.TaiLieuTacGia).FirstOrDefault(t => t.MaTaiLieu == id);
        if (t == null)
        {
            error = "Không tìm thấy tài liệu.";
            return false;
        }

        if (db.ChiTietMuons.Any(c => c.MaBanSaoNavigation.MaTaiLieu == id) || db.DatTruocs.Any(c => c.MaTaiLieu == id))
        {
            error = "Tài liệu có lịch sử mượn/đặt trước, không thể xóa.";
            return false;
        }

        try
        {
            db.BanSaos.RemoveRange(t.BanSaos);
            db.TaiLieuTacGia.RemoveRange(t.TaiLieuTacGia);
            db.TaiLieus.Remove(t);
            db.SaveChanges();
            return true;
        }
        catch (DbUpdateException)
        {
            error = "Tài liệu đang được tham chiếu, không thể xóa.";
            return false;
        }
    }
}
