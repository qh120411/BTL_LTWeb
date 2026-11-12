using System.Text.RegularExpressions;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

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
        private readonly QLThuVienUtcContext _dbContext;

        public BookCatalogService(QLThuVienUtcContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<TaiLieu> GetBaseTaiLieuQuery()
        {
            return _dbContext.TaiLieus
                .AsNoTracking()
                .Include(t => t.MaNhaXuatBanNavigation)
                .Include(t => t.MaTheLoaiNavigation)
                .Include(t => t.TaiLieuTacGia)
                    .ThenInclude(tg => tg.MaTacGiaNavigation)
                .Include(t => t.BanSaos);
        }

        private BookItem MapToBookItem(TaiLieu t)
        {
            var authorNames = t.TaiLieuTacGia != null && t.TaiLieuTacGia.Any()
                ? string.Join(", ", t.TaiLieuTacGia.Select(tg => tg.MaTacGiaNavigation?.HoTen).Where(h => !string.IsNullOrEmpty(h)))
                : "Đang cập nhật";
            if (string.IsNullOrWhiteSpace(authorNames))
            {
                authorNames = "Đang cập nhật";
            }

            var publisherName = t.MaNhaXuatBanNavigation?.TenNhaXuatBan ?? "NXB Đại học Giao thông Vận tải";
            var theLoaiName = t.MaTheLoaiNavigation?.TenTheLoai ?? "Chung";

            // Ánh xạ mã chuyên ngành (Major)
            string majorCode = "cntt";
            string tlLower = theLoaiName.ToLower();
            if (tlLower.Contains("kinh tế") || tlLower.Contains("logistics"))
                majorCode = "kinhte";
            else if (tlLower.Contains("công trình") || tlLower.Contains("xây dựng") || tlLower.Contains("giao thông"))
                majorCode = "xaydung";
            else if (tlLower.Contains("toán") || tlLower.Contains("khoa học cơ bản"))
                majorCode = "toanly";
            else if (tlLower.Contains("chính trị") || tlLower.Contains("pháp luật"))
                majorCode = "chinhtri";
            else if (tlLower.Contains("thông tin") || tlLower.Contains("cntt"))
                majorCode = "cntt";

            // Ánh xạ định dạng học liệu (Category)
            string catCode = "books";
            string format = t.DinhDang ?? "Sách in";
            if (format.Contains("Luận văn") || format.Contains("Đồ án") || format.Contains("Khóa luận"))
                catCode = "thesis";
            else if (format.Contains("Tạp chí") || format.Contains("Kỷ yếu") || format.Contains("Bài báo"))
                catCode = "journal";

            var copies = (t.BanSaos ?? new List<BanSao>()).Select(b => new BookCopyItem
            {
                Id = b.MaBanSao,
                BookId = b.MaTaiLieu,
                Barcode = b.MaVach,
                Shelf = b.ViTriKe ?? "Kho tầng 2",
                Condition = b.TinhTrang ?? "Tốt",
                Status = (b.TrangThai == "Có sẵn" || b.TrangThai == "Sẵn sàng") ? "Có sẵn" : "Đang mượn",
                ImportDate = b.NgayNhap.HasValue ? b.NgayNhap.Value.ToString("dd/MM/yyyy") : "01/01/2024"
            }).OrderBy(c => c.Barcode).ToList();

            int totalCopies = copies.Count;
            int availableCopies = copies.Count(c => c.Status == "Có sẵn");

            return new BookItem
            {
                Id = t.MaTaiLieu,
                Title = t.TenTaiLieu,
                Authors = authorNames,
                Publisher = publisherName,
                Category = catCode,
                CategoryName = format,
                Major = majorCode,
                MajorName = theLoaiName,
                Language = (t.NgonNgu != null && t.NgonNgu.ToLower().Contains("anh")) ? "eng" : "vie",
                LanguageName = t.NgonNgu ?? "Tiếng Việt",
                Year = t.NamXuatBan ?? 2023,
                Pages = t.SoTrang ?? 200,
                ISBN = t.Isbn ?? "",
                CoverImage = string.IsNullOrWhiteSpace(t.AnhBia) ? "/images/covers/sach_01.jpg" : t.AnhBia,
                TotalCopies = totalCopies,
                AvailableCopies = availableCopies,
                Shelf = copies.FirstOrDefault()?.Shelf ?? "Kho tầng 2",
                Description = t.MoTa ?? "",
                IsOnline = format.Contains("điện tử") || format.Contains("số"),
                Copies = copies
            };
        }

        public IEnumerable<BookItem> GetAll()
        {
            var list = GetBaseTaiLieuQuery().OrderBy(t => t.MaTaiLieu).ToList();
            return list.Select(MapToBookItem);
        }

        public BookItem? GetById(int id)
        {
            var entity = GetBaseTaiLieuQuery().FirstOrDefault(t => t.MaTaiLieu == id);
            return entity == null ? null : MapToBookItem(entity);
        }

        public IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability)
        {
            var all = GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                all = all.Where(b =>
                    b.Title.ToLower().Contains(kw) ||
                    b.Authors.ToLower().Contains(kw) ||
                    b.Publisher.ToLower().Contains(kw) ||
                    b.ISBN.ToLower().Contains(kw) ||
                    b.Description.ToLower().Contains(kw) ||
                    b.MajorName.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "all")
            {
                all = all.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(major) && major != "all")
            {
                all = all.Where(b => b.Major.Equals(major, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(language) && language != "all")
            {
                all = all.Where(b => b.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(availability) && availability == "available")
            {
                all = all.Where(b => b.AvailableCopies > 0);
            }

            return all.ToList();
        }

        public bool AddBook(BookItem book, int copiesCount, out int newId, out string errorMessage)
        {
            errorMessage = string.Empty;
            newId = 0;

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                errorMessage = "Tiêu đề học liệu không được để trống!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(book.Authors))
            {
                errorMessage = "Tên tác giả không được để trống!";
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                // 1. Xử lý Thể loại
                int? maTheLoai = null;
                if (!string.IsNullOrWhiteSpace(book.MajorName))
                {
                    var tl = _dbContext.TheLoais.FirstOrDefault(x => x.TenTheLoai == book.MajorName);
                    if (tl == null)
                    {
                        tl = new TheLoai { TenTheLoai = book.MajorName };
                        _dbContext.TheLoais.Add(tl);
                        _dbContext.SaveChanges();
                    }
                    maTheLoai = tl.MaTheLoai;
                }

                // 2. Xử lý Nhà xuất bản
                int? maNxb = null;
                var pubName = string.IsNullOrWhiteSpace(book.Publisher) ? "NXB Đại học Giao thông Vận tải" : book.Publisher.Trim();
                var nxb = _dbContext.NhaXuatBans.FirstOrDefault(x => x.TenNhaXuatBan == pubName);
                if (nxb == null)
                {
                    nxb = new NhaXuatBan { TenNhaXuatBan = pubName };
                    _dbContext.NhaXuatBans.Add(nxb);
                    _dbContext.SaveChanges();
                }
                maNxb = nxb.MaNhaXuatBan;

                // 3. Tạo mới TaiLieu
                var taiLieu = new TaiLieu
                {
                    TenTaiLieu = book.Title.Trim(),
                    MaNhaXuatBan = maNxb,
                    MaTheLoai = maTheLoai,
                    NgonNgu = string.IsNullOrWhiteSpace(book.LanguageName) ? "Tiếng Việt" : book.LanguageName,
                    DinhDang = string.IsNullOrWhiteSpace(book.CategoryName) ? "Sách in" : book.CategoryName,
                    NamXuatBan = (short)(book.Year > 0 ? book.Year : DateTime.Now.Year),
                    LanXuatBan = 1,
                    SoTrang = book.Pages > 0 ? book.Pages : 250,
                    Isbn = string.IsNullOrWhiteSpace(book.ISBN) ? $"978-604-0-{new Random().Next(1000, 9999)}-{new Random().Next(1, 9)}" : book.ISBN.Trim(),
                    AnhBia = string.IsNullOrWhiteSpace(book.CoverImage) ? "/images/covers/sach_01.jpg" : book.CoverImage,
                    MoTa = book.Description ?? string.Empty,
                    NgayTao = DateTime.Now
                };

                _dbContext.TaiLieus.Add(taiLieu);
                _dbContext.SaveChanges();

                newId = taiLieu.MaTaiLieu;

                // 4. Xử lý Tác giả
                var authorNames = book.Authors.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in authorNames)
                {
                    var cleanName = name.Trim();
                    if (string.IsNullOrEmpty(cleanName)) continue;

                    var author = _dbContext.TacGia.FirstOrDefault(a => a.HoTen == cleanName);
                    if (author == null)
                    {
                        author = new TacGium { HoTen = cleanName };
                        _dbContext.TacGia.Add(author);
                        _dbContext.SaveChanges();
                    }

                    _dbContext.TaiLieuTacGia.Add(new TaiLieuTacGium
                    {
                        MaTaiLieu = newId,
                        MaTacGia = author.MaTacGia,
                        VaiTroTacGia = "Tác giả chính"
                    });
                }
                _dbContext.SaveChanges();

                // 5. Tạo các bản sao vật lý (BanSao)
                int totalCopies = Math.Max(1, copiesCount);
                var shelf = string.IsNullOrWhiteSpace(book.Shelf) ? "Kho tầng 2" : book.Shelf.Trim();

                for (int i = 1; i <= totalCopies; i++)
                {
                    _dbContext.BanSaos.Add(new BanSao
                    {
                        MaTaiLieu = newId,
                        MaVach = $"UTC-TL{newId:D3}-{i:D2}",
                        ViTriKe = shelf,
                        TinhTrang = "Mới",
                        TrangThai = "Có sẵn",
                        NgayNhap = DateOnly.FromDateTime(DateTime.Today)
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                errorMessage = $"Lỗi khi thêm học liệu vào CSDL: {ex.Message}";
                return false;
            }
        }

        public bool UpdateBook(BookItem book, out string errorMessage)
        {
            errorMessage = string.Empty;

            var existing = _dbContext.TaiLieus
                .Include(t => t.BanSaos)
                .Include(t => t.TaiLieuTacGia)
                .FirstOrDefault(t => t.MaTaiLieu == book.Id);

            if (existing == null)
            {
                errorMessage = "Không tìm thấy học liệu trong CSDL!";
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                existing.TenTaiLieu = book.Title.Trim();
                existing.NamXuatBan = (short)(book.Year > 0 ? book.Year : existing.NamXuatBan ?? 2023);
                existing.SoTrang = book.Pages > 0 ? book.Pages : existing.SoTrang;
                existing.Isbn = book.ISBN;
                existing.MoTa = book.Description;
                if (!string.IsNullOrWhiteSpace(book.CoverImage))
                {
                    existing.AnhBia = book.CoverImage;
                }

                // Cập nhật NXB
                if (!string.IsNullOrWhiteSpace(book.Publisher))
                {
                    var nxb = _dbContext.NhaXuatBans.FirstOrDefault(n => n.TenNhaXuatBan == book.Publisher.Trim());
                    if (nxb == null)
                    {
                        nxb = new NhaXuatBan { TenNhaXuatBan = book.Publisher.Trim() };
                        _dbContext.NhaXuatBans.Add(nxb);
                        _dbContext.SaveChanges();
                    }
                    existing.MaNhaXuatBan = nxb.MaNhaXuatBan;
                }

                // Cập nhật Thể loại
                if (!string.IsNullOrWhiteSpace(book.MajorName))
                {
                    var tl = _dbContext.TheLoais.FirstOrDefault(t => t.TenTheLoai == book.MajorName.Trim());
                    if (tl == null)
                    {
                        tl = new TheLoai { TenTheLoai = book.MajorName.Trim() };
                        _dbContext.TheLoais.Add(tl);
                        _dbContext.SaveChanges();
                    }
                    existing.MaTheLoai = tl.MaTheLoai;
                }

                // Cập nhật vị trí kệ trên các bản sao
                if (!string.IsNullOrWhiteSpace(book.Shelf) && existing.BanSaos != null)
                {
                    foreach (var c in existing.BanSaos)
                    {
                        c.ViTriKe = book.Shelf.Trim();
                    }
                }

                // Cập nhật Tác giả
                if (!string.IsNullOrWhiteSpace(book.Authors))
                {
                    // Xóa liên kết cũ
                    if (existing.TaiLieuTacGia != null)
                    {
                        _dbContext.TaiLieuTacGia.RemoveRange(existing.TaiLieuTacGia);
                        _dbContext.SaveChanges();
                    }

                    var authorNames = book.Authors.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var name in authorNames)
                    {
                        var cleanName = name.Trim();
                        if (string.IsNullOrEmpty(cleanName)) continue;

                        var author = _dbContext.TacGia.FirstOrDefault(a => a.HoTen == cleanName);
                        if (author == null)
                        {
                            author = new TacGium { HoTen = cleanName };
                            _dbContext.TacGia.Add(author);
                            _dbContext.SaveChanges();
                        }

                        _dbContext.TaiLieuTacGia.Add(new TaiLieuTacGium
                        {
                            MaTaiLieu = existing.MaTaiLieu,
                            MaTacGia = author.MaTacGia,
                            VaiTroTacGia = "Tác giả"
                        });
                    }
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                errorMessage = $"Lỗi khi cập nhật học liệu: {ex.Message}";
                return false;
            }
        }

        public bool DeleteBook(int id, out string errorMessage)
        {
            errorMessage = string.Empty;

            var existing = _dbContext.TaiLieus
                .Include(t => t.BanSaos)
                .Include(t => t.TaiLieuTacGia)
                .FirstOrDefault(t => t.MaTaiLieu == id);

            if (existing == null)
            {
                errorMessage = "Không tìm thấy học liệu trong CSDL!";
                return false;
            }

            // Kiểm tra có bản sao nào đang được mượn không
            bool hasBorrowedCopies = _dbContext.ChiTietMuons.Any(ct =>
                ct.MaBanSaoNavigation.MaTaiLieu == id &&
                (ct.TrangThai == "Đang mượn" || ct.TrangThai == "Quá hạn"));

            if (hasBorrowedCopies || existing.BanSaos.Any(b => b.TrangThai == "Đang mượn"))
            {
                errorMessage = $"Không thể xóa học liệu '{existing.TenTaiLieu}' vì hiện đang có bản sao được mượn lưu hành ngoài kho!";
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                // Xóa chi tiết mượn lịch sử cũ của các bản sao này (nếu có và đã trả xong)
                var banSaoIds = existing.BanSaos.Select(b => b.MaBanSao).ToList();
                var oldTransactions = _dbContext.ChiTietMuons.Where(ct => banSaoIds.Contains(ct.MaBanSao)).ToList();
                if (oldTransactions.Any())
                {
                    _dbContext.ChiTietMuons.RemoveRange(oldTransactions);
                }

                // Xóa các bản sao
                _dbContext.BanSaos.RemoveRange(existing.BanSaos);

                // Xóa liên kết tác giả
                _dbContext.TaiLieuTacGia.RemoveRange(existing.TaiLieuTacGia);

                // Xóa tài liệu
                _dbContext.TaiLieus.Remove(existing);

                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                errorMessage = $"Lỗi khi xóa học liệu: {ex.Message}";
                return false;
            }
        }

        public List<BookCopyItem> GetCopiesByBookId(int bookId)
        {
            return _dbContext.BanSaos
                .AsNoTracking()
                .Where(b => b.MaTaiLieu == bookId)
                .Select(b => new BookCopyItem
                {
                    Id = b.MaBanSao,
                    BookId = b.MaTaiLieu,
                    Barcode = b.MaVach,
                    Shelf = b.ViTriKe ?? "Kho tầng 2",
                    Condition = b.TinhTrang ?? "Tốt",
                    Status = (b.TrangThai == "Có sẵn" || b.TrangThai == "Sẵn sàng") ? "Có sẵn" : "Đang mượn",
                    ImportDate = b.NgayNhap.HasValue ? b.NgayNhap.Value.ToString("dd/MM/yyyy") : "01/01/2024"
                })
                .OrderBy(c => c.Barcode)
                .ToList();
        }
    }
}
