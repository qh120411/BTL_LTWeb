using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;
using BTL_LTWEB.Areas.Reader.Models;

namespace BTL_LTWEB.Areas.Reader.Services
{
    public interface IBookCatalogService
    {
        IEnumerable<BookItem> GetAll();
        BookItem? GetById(int id);
        IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability);

        // Nghiệp vụ Quản lý học liệu CSDL Thư viện UTC
        bool AddBook(BookItem book, int copiesCount, out int newId, out string errorMessage);
        bool UpdateBook(BookItem book, out string errorMessage);
        bool DeleteBook(int id, out string errorMessage);
        List<BookCopyItem> GetCopiesByBookId(int bookId);
    }

    public class BookCatalogService : IBookCatalogService
    {
        private readonly QLThuVienUtcContext _db;
        public BookCatalogService(QLThuVienUtcContext db) => _db = db;
        private IQueryable<TaiLieu> Query() => _db.TaiLieus.AsNoTracking()
            .Include(t => t.BanSaos).Include(t => t.MaTheLoaiNavigation).Include(t => t.MaNhaXuatBanNavigation)
            .Include(t => t.TaiLieuTacGia).ThenInclude(a => a.MaTacGiaNavigation).AsSplitQuery();
        private static string MajorCode(string name) => name.Contains("thông tin", StringComparison.OrdinalIgnoreCase) ? "cntt" :
            name.Contains("công trình", StringComparison.OrdinalIgnoreCase) || name.Contains("xây dựng", StringComparison.OrdinalIgnoreCase) ? "xaydung" :
            name.Contains("Kinh tế", StringComparison.OrdinalIgnoreCase) ? "kinhte" : name.Contains("chính trị", StringComparison.OrdinalIgnoreCase) ? "chinhtri" : name.Contains("Khoa học", StringComparison.OrdinalIgnoreCase) || name.Contains("Toán", StringComparison.OrdinalIgnoreCase) ? "toanly" : string.Empty;
        private static BookItem Map(TaiLieu t)
        {
            var category = (t.Isbn ?? "").StartsWith("LV-", StringComparison.OrdinalIgnoreCase) || (t.MoTa ?? "").StartsWith("Luận văn") ? "thesis" :
                (t.Isbn ?? "").StartsWith("ISSN", StringComparison.OrdinalIgnoreCase) || t.TenTaiLieu.Contains("Tạp chí", StringComparison.OrdinalIgnoreCase) ? "journal" : "books";
            var copies = t.BanSaos.OrderBy(c => c.MaBanSao).Select(c => new BookCopyItem
            {
                Id = c.MaBanSao,
                BookId = t.MaTaiLieu,
                Barcode = c.MaVach,
                Shelf = c.ViTriKe ?? "",
                Condition = c.TinhTrang,
                Status = c.TrangThai,
                ImportDate = c.NgayNhap?.ToString("dd/MM/yyyy") ?? ""
            }).ToList();
            return new BookItem
            {
                Id = t.MaTaiLieu,
                Title = t.TenTaiLieu,
                Authors = string.Join(", ", t.TaiLieuTacGia.OrderBy(a => a.MaTacGia).Select(a => a.MaTacGiaNavigation.HoTen)),
                Publisher = t.MaNhaXuatBanNavigation?.TenNhaXuatBan ?? "",
                ISBN = t.Isbn ?? "",
                Year = t.NamXuatBan ?? 0,
                Pages = t.SoTrang ?? 0,
                Major = MajorCode(t.MaTheLoaiNavigation?.TenTheLoai ?? ""),
                MajorName = t.MaTheLoaiNavigation?.TenTheLoai ?? "",
                Category = category,
                CategoryName = category == "thesis" ? "Luận văn & Luận án" : category == "journal" ? "Báo & Tạp chí" : "Sách & Giáo trình",
                Language = (t.NgonNgu ?? "").Contains("Anh", StringComparison.OrdinalIgnoreCase) ? "eng" : (t.NgonNgu ?? "").Contains("Việt", StringComparison.OrdinalIgnoreCase) ? "vie" : string.Empty,
                LanguageName = t.NgonNgu ?? "",
                IsOnline = (t.DinhDang ?? "").Contains("điện tử", StringComparison.OrdinalIgnoreCase),
                Description = t.MoTa ?? "",
                CoverImage = t.AnhBia ?? "",
                Copies = copies,
                TotalCopies = copies.Count,
                AvailableCopies = copies.Count(c => c.IsAvailable),
                Shelf = copies.FirstOrDefault()?.Shelf ?? ""
            };
        }
        public IEnumerable<BookItem> GetAll() => Query().OrderBy(t => t.MaTaiLieu).ToList().Select(Map).ToList();
        public BookItem? GetById(int id)
        {
            var document = Query().SingleOrDefault(t => t.MaTaiLieu == id);
            if (document == null) return null;
            var book = Map(document);
            var dueDates = _db.ChiTietMuons.AsNoTracking()
                .Where(c => c.MaBanSaoNavigation.MaTaiLieu == id && c.NgayTra == null &&
                    c.TrangThai != "Đã trả" && c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền")
                .GroupBy(c => c.MaBanSao)
                .Select(g => new { CopyId = g.Key, DueDate = g.Max(c => c.HanTra) })
                .ToDictionary(c => c.CopyId, c => c.DueDate);
            foreach (var copy in book.Copies)
                copy.DueDate = dueDates.TryGetValue(copy.Id, out var dueDate) ? dueDate : null;
            return book;
        }
        public List<BookCopyItem> GetCopiesByBookId(int id) => GetById(id)?.Copies ?? new();
        public IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability)
        {
            var query = GetAll();
            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(b => new[] { b.Title, b.Authors, b.Publisher, b.ISBN, b.Description, b.MajorName }.Any(s => s.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase)));
            if (!string.IsNullOrWhiteSpace(category) && category != "all") query = query.Where(b => b.Category == category);
            if (!string.IsNullOrWhiteSpace(major) && major != "all") query = query.Where(b => b.Major == major);
            if (!string.IsNullOrWhiteSpace(language) && language != "all") query = query.Where(b => b.Language == language);
            if (availability == "available") query = query.Where(b => b.AvailableCopies > 0);
            return query.ToList();
        }
        private void SetMetadata(TaiLieu t, BookItem b)
        {
            t.TenTaiLieu = b.Title.Trim(); t.Isbn = string.IsNullOrWhiteSpace(b.ISBN) ? null : b.ISBN.Trim();
            t.NamXuatBan = (short)Math.Clamp(b.Year, 0, short.MaxValue); t.SoTrang = b.Pages; t.MoTa = b.Description; t.AnhBia = b.CoverImage;
            t.NgonNgu = string.IsNullOrWhiteSpace(b.LanguageName) ? null : b.LanguageName.Trim();
            t.MaTheLoaiNavigation = _db.TheLoais.Local.FirstOrDefault(x => MajorCode(x.TenTheLoai) == b.Major) ?? _db.TheLoais.ToList().FirstOrDefault(x => MajorCode(x.TenTheLoai) == b.Major);
            t.MaNhaXuatBanNavigation = string.IsNullOrWhiteSpace(b.Publisher) ? null :
                _db.NhaXuatBans.Local.FirstOrDefault(x => x.TenNhaXuatBan == b.Publisher) ?? _db.NhaXuatBans.FirstOrDefault(x => x.TenNhaXuatBan == b.Publisher) ?? new NhaXuatBan { TenNhaXuatBan = b.Publisher.Trim() };
            var names = b.Authors.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
            foreach (var old in t.TaiLieuTacGia.ToList())
            {
                var oldName = old.MaTacGiaNavigation?.HoTen ?? _db.TacGia.Find(old.MaTacGia)?.HoTen;
                if (!names.Contains(oldName ?? "")) { _db.TaiLieuTacGia.Remove(old); t.TaiLieuTacGia.Remove(old); }
            }
            foreach (var name in b.Authors.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct())
            {
                var author = _db.TacGia.Local.FirstOrDefault(a => a.HoTen == name) ?? _db.TacGia.FirstOrDefault(a => a.HoTen == name) ?? new TacGium { HoTen = name };
                if (!t.TaiLieuTacGia.Any(a => a.MaTacGia == author.MaTacGia && author.MaTacGia != 0)) t.TaiLieuTacGia.Add(new TaiLieuTacGium { MaTacGiaNavigation = author, VaiTroTacGia = "Tác giả" });
            }
        }
        private static bool Valid(BookItem b, out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(b.Title) || string.IsNullOrWhiteSpace(b.Authors)) error = "Vui lòng nhập tên học liệu và tác giả.";
            else if (b.Year < 1 || b.Year > 9999 || b.Pages <= 0 ||
                !new[] { "cntt", "xaydung", "kinhte", "toanly", "chinhtri" }.Contains(b.Major) ||
                (!b.IsOnline && string.IsNullOrWhiteSpace(b.Shelf))) error = "Thông tin chuyên ngành, năm xuất bản, số trang hoặc vị trí kệ không hợp lệ.";
            else if (b.Title.Length > 255 || b.ISBN.Length > 20 || b.Publisher.Length > 150 || b.LanguageName.Length > 100 || b.Authors.Split(',').Any(a => a.Trim().Length > 100)) error = "Thông tin học liệu vượt độ dài cho phép.";
            return error.Length == 0;
        }
        public bool AddBook(BookItem book, int copiesCount, out int newId, out string errorMessage)
        {
            newId = 0; if (!Valid(book, out errorMessage)) return false;
            if (copiesCount < 0 || copiesCount > 1000 || (!book.IsOnline && copiesCount == 0)) { errorMessage = "Số bản sách không hợp lệ."; return false; }
            try
            {
                using var tx = _db.Database.BeginTransaction();
                var t = new TaiLieu { NgayTao = DateTime.Now, NgonNgu = book.LanguageName, DinhDang = book.IsOnline ? "Bản điện tử" : "Sách in" };
                SetMetadata(t, book); _db.TaiLieus.Add(t); _db.SaveChanges();
                if (!book.IsOnline) for (int i = 1; i <= copiesCount; i++) t.BanSaos.Add(new BanSao { MaVach = $"UTC-{t.MaTaiLieu:D3}-{i:D3}", ViTriKe = book.Shelf, TinhTrang = "Mới", TrangThai = "Có sẵn", NgayNhap = DateOnly.FromDateTime(DateTime.Today) });
                _db.SaveChanges(); tx.Commit(); newId = t.MaTaiLieu; book.Id = newId; return true;
            }
            catch (DbUpdateException) { _db.ChangeTracker.Clear(); errorMessage = "Không lưu được học liệu vào SQL Server."; return false; }
        }
        public bool UpdateBook(BookItem book, out string errorMessage)
        {
            if (!Valid(book, out errorMessage)) return false;
            try
            {
                using var tx = _db.Database.BeginTransaction();
                var t = _db.TaiLieus.Include(t => t.BanSaos).Include(t => t.TaiLieuTacGia).SingleOrDefault(t => t.MaTaiLieu == book.Id);
                if (t == null) { errorMessage = "Không tìm thấy học liệu."; return false; }
                SetMetadata(t, book); foreach (var c in t.BanSaos) c.ViTriKe = book.Shelf;
                _db.SaveChanges(); tx.Commit(); return true;
            }
            catch (DbUpdateException) { _db.ChangeTracker.Clear(); errorMessage = "Không cập nhật được học liệu vào SQL Server."; return false; }
        }
        public bool DeleteBook(int id, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using var tx = _db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
                var t = _db.TaiLieus.Include(t => t.BanSaos).Include(t => t.TaiLieuTacGia).SingleOrDefault(t => t.MaTaiLieu == id);
                if (t == null) { errorMessage = "Không tìm thấy học liệu."; return false; }
                if (_db.ChiTietMuons.Any(c => c.MaBanSaoNavigation.MaTaiLieu == id) || t.BanSaos.Any(c => c.TrangThai != "Có sẵn")) { errorMessage = "Không thể xóa học liệu có lịch sử mượn hoặc bản sách không sẵn có."; return false; }
                _db.BanSaos.RemoveRange(t.BanSaos); _db.TaiLieuTacGia.RemoveRange(t.TaiLieuTacGia); _db.TaiLieus.Remove(t);
                _db.SaveChanges(); tx.Commit(); return true;
            }
            catch (DbUpdateException) { _db.ChangeTracker.Clear(); errorMessage = "Học liệu đang được dữ liệu khác tham chiếu hoặc chưa kết nối được SQL Server."; return false; }
        }
    }
}
