using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Services;

public class ReaderService : IReaderService
{
    private readonly QLThuVienUtcContext _dbContext;
    private readonly IBookCatalogService _catalogService;
    private readonly TimeProvider _timeProvider;
    public ReaderService(QLThuVienUtcContext dbContext, IBookCatalogService catalogService, TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext; _catalogService = catalogService; _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }

    public async Task<List<BorrowReaderSearchItem>> SearchBorrowReadersAsync(string? query, CancellationToken cancellationToken = default)
    {
        var keyword = query?.Trim() ?? string.Empty;
        if (keyword.Length < 2 || keyword.Length > 100) return new();

        // Filter and limit in SQL; never load the school-wide directory into the modal.
        const string searchCollation = "Latin1_General_100_CI_AI";
        var normalizedKeyword = RemoveDiacritics(keyword);
        var matches = await _dbContext.BanDocs.AsNoTracking()
            .Where(b => EF.Functions.Collate(b.MaSinhVien, searchCollation).Contains(keyword) ||
                        EF.Functions.Collate(b.HoTen, searchCollation).Replace("đ", "d").Replace("Đ", "D").Contains(normalizedKeyword))
            .OrderByDescending(b => b.MaSinhVien == keyword)
            .ThenByDescending(b => b.MaSinhVien.StartsWith(keyword))
            .ThenBy(b => b.HoTen).ThenBy(b => b.MaSinhVien)
            .Take(10)
            .Select(b => new BorrowReaderSearchItem
            {
                ReaderId = b.MaBanDoc,
                MaBD = b.MaSinhVien,
                HoTen = b.HoTen,
                LopKhoa = b.MaLopNavigation == null ? "" : b.MaLopNavigation.TenLop
            })
            .ToListAsync(cancellationToken);

        if (matches.Count == 0) return matches;

        var readerIds = matches.Select(b => b.ReaderId).ToList();
        var activeLoans = await _dbContext.ChiTietMuons.AsNoTracking()
            .Where(c => c.MaPhieuMuonNavigation.MaBanDoc.HasValue &&
                        readerIds.Contains(c.MaPhieuMuonNavigation.MaBanDoc.Value) &&
                        c.NgayTra == null && c.TrangThai != "Đã trả" &&
                        c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền")
            .Select(c => new
            {
                ReaderId = c.MaPhieuMuonNavigation.MaBanDoc,
                Title = c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu
            }).ToListAsync(cancellationToken);

        foreach (var reader in matches)
        {
            var loans = activeLoans.Where(c => c.ReaderId == reader.ReaderId).ToList();
            reader.SoSachDangMuon = loans.Count;
            reader.BorrowedTitles = loans.Select(c => c.Title).Distinct().ToList();
            reader.GioiHanSach = reader.MaBD.StartsWith("GV", StringComparison.OrdinalIgnoreCase) ||
                                 reader.MaBD.StartsWith("CB", StringComparison.OrdinalIgnoreCase) ? 10 : 5;
        }

        return matches;
    }

    public ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup)
    {


        var banDocs = _dbContext.BanDocs
            .Include(b => b.MaLopNavigation)
            .Include(b => b.PhieuMuons)
                .ThenInclude(p => p.ChiTietMuons)
            .AsNoTracking()
            .ToList();

        var today = DateTime.Today;
        var list = new List<ReaderListItemViewModel>();

        foreach (var b in banDocs)
        {
            var isLecturer = b.MaSinhVien.StartsWith("GV", StringComparison.OrdinalIgnoreCase) ||
                             b.MaSinhVien.StartsWith("CB", StringComparison.OrdinalIgnoreCase);
            var group = isLecturer ? "Giảng viên" : "Sinh viên";

            var activeLoans = b.PhieuMuons
                .SelectMany(p => p.ChiTietMuons)
                .Where(c => c.NgayTra == null &&
                            c.TrangThai != "Đã trả" &&
                            c.TrangThai != "Đã đền sách mới" &&
                            c.TrangThai != "Đã đền tiền")
                .ToList() ?? new List<ChiTietMuon>();

            int overdueCount = activeLoans.Count(c => c.TrangThai == "Quá hạn" ||
                                                    c.HanTra < DateOnly.FromDateTime(today));

            string lopKhoa = b.MaLopNavigation != null
                ? $"{b.MaLopNavigation.TenLop} · {b.MaLopNavigation.KhoaHoc ?? string.Empty}"
                : string.Empty;

            list.Add(new ReaderListItemViewModel
            {
                MaBD = b.MaSinhVien,
                HoTen = b.HoTen,
                Nhom = group,
                LopKhoa = lopKhoa,
                QuyenSuDung = b.TrangThai == "Hoạt động" ? "Hoạt động" : "Đang khóa",
                SoSachDangMuon = activeLoans.Count,
                SoSachQuaHan = overdueCount
            });
        }

        var query = list.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(selectedGroup) && selectedGroup != "Tất cả nhóm")
        {
            query = query.Where(r => r.Nhom.Equals(selectedGroup, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var kw = searchKeyword.Trim().ToLower();
            var kwNoDiacritics = RemoveDiacritics(kw);
            query = query.Where(r => r.MaBD.ToLower().Contains(kw) ||
                                     r.HoTen.ToLower().Contains(kw) ||
                                     RemoveDiacritics(r.HoTen.ToLower()).Contains(kwNoDiacritics) ||
                                     r.LopKhoa.ToLower().Contains(kw) ||
                                     RemoveDiacritics(r.LopKhoa.ToLower()).Contains(kwNoDiacritics));
        }

        return new ReaderIndexViewModel
        {
            SearchKeyword = searchKeyword,
            SelectedGroup = selectedGroup ?? "Tất cả nhóm",
            Readers = query.ToList()
        };

    }

    public ReaderDetailViewModel? GetReaderById(string id)
    {


        var b = _dbContext.BanDocs
            .Include(b => b.MaLopNavigation)
            .Include(b => b.PhieuMuons)
                .ThenInclude(p => p.ChiTietMuons)
                    .ThenInclude(c => c.MaBanSaoNavigation)
                            .ThenInclude(bs => bs.MaTaiLieuNavigation)
                                .ThenInclude(tl => tl.TaiLieuTacGia)
                                    .ThenInclude(tg => tg.MaTacGiaNavigation)
            .AsNoTracking()
            .FirstOrDefault(bd => bd.MaSinhVien == id);

        if (b == null) return null;

        var today = DateTime.Today;
        var isLecturer = b.MaSinhVien.StartsWith("GV", StringComparison.OrdinalIgnoreCase) ||
                         b.MaSinhVien.StartsWith("CB", StringComparison.OrdinalIgnoreCase);

        var activeLoans = b.PhieuMuons
            .SelectMany(p => p.ChiTietMuons)
            .Where(c => c.NgayTra == null &&
                        c.TrangThai != "Đã trả" &&
                        c.TrangThai != "Đã đền sách mới" &&
                        c.TrangThai != "Đã đền tiền")
            .Select(c =>
            {
                var book = c.MaBanSaoNavigation?.MaTaiLieuNavigation;
                var hanTraDate = c.HanTra.ToDateTime(TimeOnly.MinValue);
                var isOverdue = today > hanTraDate || c.TrangThai == "Quá hạn";
                var daysOverdue = isOverdue ? Math.Max(1, (int)(today - hanTraDate).TotalDays) : 0;

                var authors = (book != null && book.TaiLieuTacGia.Any())
                    ? string.Join(", ", book.TaiLieuTacGia.Select(tg => tg.MaTacGiaNavigation.HoTen))
                    : "Nhiều tác giả";

                var ngayMuonDate = c.MaPhieuMuonNavigation != null ? c.MaPhieuMuonNavigation.NgayMuon : DateTime.Now;

                return new BorrowTransactionViewModel
                {
                    MaGD = $"GD{ngayMuonDate:yyMMdd}-{c.MaChiTietMuon:D3}",
                    MaSach = c.MaBanSaoNavigation?.MaVach ?? $"BS-{c.MaBanSao:D5}",
                    TenSach = book?.TenTaiLieu ?? "Học liệu UTC",
                    TacGia = authors,
                    CoverImage = book?.AnhBia ?? "/images/covers/sach_01.jpg",
                    NgayMuon = ngayMuonDate.ToString("dd/MM/yyyy"),
                    NgayHenTra = c.HanTra.ToString("dd/MM/yyyy"),
                    TrangThai = isOverdue ? $"Quá hạn {daysOverdue} ngày" : "Còn hạn",
                    IsQuaHan = isOverdue,
                    SoNgayQuaHan = daysOverdue,
                    TienPhat = isOverdue ? 100000m : 0m
                };
            }).ToList() ?? new List<BorrowTransactionViewModel>();

        var availableBooks = _catalogService.GetAll().ToList();

        string lopKhoa = b.MaLopNavigation != null
            ? $"{b.MaLopNavigation.TenLop} · {b.MaLopNavigation.KhoaHoc ?? string.Empty}"
            : string.Empty;

        return new ReaderDetailViewModel
        {
            MaBD = b.MaSinhVien,
            HoTen = b.HoTen,
            LopKhoa = lopKhoa,
            TrangThaiHocTap = isLecturer ? "Đang công tác" : "Đang học",
            Nhom = isLecturer ? "Giảng viên" : "Sinh viên",
            QuyenSuDung = b.TrangThai == "Hoạt động" ? "Hoạt động" : "Đang khóa",
            GioiHanSach = isLecturer ? "10 cuốn" : "5 cuốn",
            HieuLucQuyen = b.NgayHetHanThe?.ToString("yyyy-MM-dd") ?? string.Empty,
            BorrowTransactions = activeLoans,
            AvailableBooks = availableBooks
        };

    }

    public bool UpdateReaderSettings(ReaderDetailViewModel model)
    {
        try
        {
            var banDoc = _dbContext.BanDocs.FirstOrDefault(b => b.MaSinhVien == model.MaBD);
            if (banDoc != null)
            {
                if (!string.IsNullOrWhiteSpace(model.QuyenSuDung))
                {
                    banDoc.TrangThai = model.QuyenSuDung;
                }
                _dbContext.SaveChanges();
                return true;
            }
        }
        catch { }
        return false;
    }

    private ChiTietMuon? FindActiveReturnLoan(string maBD, string maGD)
    {
        if (string.IsNullOrWhiteSpace(maBD) || string.IsNullOrWhiteSpace(maGD)) return null;
        var readerCode = maBD.Trim();
        var transactionCode = maGD.Trim();
        var query = _dbContext.ChiTietMuons
            .Include(c => c.MaPhieuMuonNavigation).ThenInclude(p => p.MaBanDocNavigation)
            .Include(c => c.MaBanSaoNavigation).ThenInclude(b => b.MaTaiLieuNavigation)
            .Where(c => c.MaPhieuMuonNavigation.MaBanDocNavigation != null &&
                        c.MaPhieuMuonNavigation.MaBanDocNavigation.MaSinhVien == readerCode &&
                        c.NgayTra == null && c.TrangThai != "Đã trả" &&
                        c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền");
        if (int.TryParse(transactionCode, out var directId))
            return query.FirstOrDefault(c => c.MaChiTietMuon == directId);
        var match = System.Text.RegularExpressions.Regex.Match(transactionCode, @"^GD\d{6}-(\d+)$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var loanId))
            return query.FirstOrDefault(c => c.MaChiTietMuon == loanId);
        return query.FirstOrDefault(c => c.MaBanSaoNavigation.MaVach == transactionCode);
    }

    public bool ReturnBook(string maBD, string maGD, out string bookTitle, out decimal reducedFine, string? staffUsername = null)
    {
        bookTitle = string.Empty;
        reducedFine = 0;
        try
        {
            var loan = FindActiveReturnLoan(maBD, maGD);
            if (loan == null) return false;
            bookTitle = loan.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu;
            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
            reducedFine = loan.HanTra < today || loan.TrangThai == "Quá hạn" ? 100000m : 0m;
            return ProcessReturnOrCompensation(maBD, maGD, "NormalReturn", "NewBook", 0, true, null, out _, staffUsername);
        }
        catch
        {
            return false;
        }
    }

    public bool ProcessReturnOrCompensation(
        string maBD, string maGD, string actionType, string compensationType,
        decimal compensationAmount, bool payOverdue, string? note, out string message,
        string? staffUsername = null)
    {
        message = string.Empty;
        bool isCompensation = string.Equals(actionType, "Compensation", StringComparison.OrdinalIgnoreCase);
        bool isCash = string.Equals(compensationType, "Cash", StringComparison.OrdinalIgnoreCase);
        if ((!isCompensation && !string.Equals(actionType, "NormalReturn", StringComparison.OrdinalIgnoreCase)) ||
            (isCompensation && !isCash && !string.Equals(compensationType, "NewBook", StringComparison.OrdinalIgnoreCase)) ||
            (isCompensation && isCash && (compensationAmount <= 0 || compensationAmount > 100000000m)))
        {
            message = "Hình thức xử lý hoặc số tiền bồi thường không hợp lệ.";
            return false;
        }

        try
        {
            using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var loan = FindActiveReturnLoan(maBD, maGD);
            if (loan == null)
            {
                message = "Không tìm thấy giao dịch đang mượn của bạn đọc này.";
                return false;
            }
            var now = _timeProvider.GetLocalNow().DateTime;
            var reader = loan.MaPhieuMuonNavigation.MaBanDocNavigation!;
            var book = loan.MaBanSaoNavigation.MaTaiLieuNavigation;
            var staff = ResolveBorrowingStaff(staffUsername);
            if (staff == null)
            {
                message = "Không xác định được cán bộ xử lý. Vui lòng đăng nhập lại.";
                return false;
            }
            var overdue = loan.HanTra < DateOnly.FromDateTime(now) || loan.TrangThai == "Quá hạn";
            var overdueFee = overdue && (!isCompensation || payOverdue) ? 100000m : 0m;
            var compensationFee = isCompensation && isCash ? compensationAmount : 0m;
            var total = overdueFee + compensationFee;
            var transactionCode = $"GD{loan.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{loan.MaChiTietMuon:D3}";

            loan.NgayTra = now;
            loan.TrangThai = isCompensation ? (isCash ? "Đã đền tiền" : "Đã đền sách mới") : "Đã trả";
            loan.TinhTrangKhiTra = isCompensation ? (isCash ? "Mất sách, bồi thường bằng tiền" : "Đã nhận sách mới tương đương") : "Tốt";
            loan.MaBanSaoNavigation.TrangThai = isCompensation && isCash ? "Đã mất" : "Có sẵn";
            loan.MaBanSaoNavigation.TinhTrang = isCompensation && isCash ? "Mất" : "Tốt";
            var hasOtherLoans = _dbContext.ChiTietMuons.Any(c => c.MaPhieuMuon == loan.MaPhieuMuon &&
                c.MaChiTietMuon != loan.MaChiTietMuon && c.NgayTra == null && c.TrangThai != "Đã trả" &&
                c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền");
            if (!hasOtherLoans) loan.MaPhieuMuonNavigation.TrangThai = "Đã trả";
            if (overdueFee > 0)
            {
                foreach (var violation in _dbContext.ViPhams.Where(v => v.MaChiTietMuon == loan.MaChiTietMuon))
                {
                    violation.TrangThaiNop = "Đã thanh toán";
                    violation.NgayNop = now;
                }
            }
            if (isCompensation || total > 0)
            {
                _dbContext.ThanhToans.Add(new ThanhToan
                {
                    MaBanDoc = reader.MaBanDoc,
                    MaNhanVienNavigation = staff,
                    NgayThanhToan = now,
                    TongTien = total,
                    PhuongThuc = isCompensation && !isCash ? "Sách thay thế" : "Tiền mặt",
                    MaChiTietMuon = loan.MaChiTietMuon,
                    TienNopMuon = overdueFee,
                    TienDenBu = compensationFee,
                    LoaiKhoanThu = isCompensation ? "Đền bù mất/hỏng" : "Nộp muộn",
                    HinhThucXuLy = isCompensation ? (isCash ? "Đền tiền" : "Đền sách mới") : "Trả sách quá hạn",
                    GhiChu = note?.Trim(),
                    TrangThai = "Thành công",
                    NoiDung = $"Thu phí lưu thông: {transactionCode} ({book.TenTaiLieu})"
                });
            }
            _dbContext.SaveChanges();
            transaction.Commit();

            message = total > 0 ? $"Đã xử lý sách và thu {total:N0} đ." : "Đã xử lý trả sách.";
            return true;
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            message = "Chưa lưu được giao dịch trả sách. Vui lòng thử lại.";
            return false;
        }
    }

    public bool BorrowBook(string maBD, int bookId, out string bookTitle, out string errorMessage, string? staffUsername = null)
    {
        bookTitle = string.Empty;
        errorMessage = string.Empty;
        var borrowedAt = _timeProvider.GetLocalNow().DateTime;
        if (!BTL_LTWEB.Common.LoanTermPolicy.TryGetDueDate(DateOnly.FromDateTime(borrowedAt), out var dueDate))
        {
            errorMessage = BTL_LTWEB.Common.LoanTermPolicy.BorrowingWindowMessage;
            return false;
        }
        var catalogBook = _catalogService.GetById(bookId);
        if (string.IsNullOrWhiteSpace(maBD) || catalogBook == null || catalogBook.IsOnline)
        {
            errorMessage = "Vui lòng chọn bạn đọc và sách in hợp lệ.";
            return false;
        }

        try
        {
            // Keep reader limits, copy reservation and loan creation in one SQL transaction.
            using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var reader = _dbContext.BanDocs
                .Include(b => b.PhieuMuons).ThenInclude(p => p.ChiTietMuons).ThenInclude(c => c.MaBanSaoNavigation)
                .FirstOrDefault(b => b.MaSinhVien == maBD.Trim());
            if (reader == null)
            {
                errorMessage = $"Không tìm thấy bạn đọc có mã '{maBD}' trong CSDL!";
                return false;
            }
            var activeLoans = reader.PhieuMuons.SelectMany(p => p.ChiTietMuons)
                .Where(c => c.NgayTra == null && c.TrangThai != "Đã trả" &&
                            c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền").ToList();
            var limit = reader.MaSinhVien.StartsWith("GV", StringComparison.OrdinalIgnoreCase) ||
                        reader.MaSinhVien.StartsWith("CB", StringComparison.OrdinalIgnoreCase) ? 10 : 5;
            if (activeLoans.Count >= limit)
            {
                errorMessage = $"Bạn đọc đã đạt giới hạn mượn tối đa ({limit} cuốn).";
                return false;
            }

            var document = _dbContext.TaiLieus.Include(d => d.BanSaos).SingleOrDefault(d => d.MaTaiLieu == bookId);
            if (document == null) { errorMessage = "Không tìm thấy học liệu trong SQL Server."; return false; }
            if (activeLoans.Any(c => c.MaBanSaoNavigation.MaTaiLieu == bookId)) { errorMessage = "Bạn đọc đã mượn đầu sách này rồi!"; return false; }
            if (!document.BanSaos.Any(c => c.TrangThai == "Có sẵn")) { errorMessage = "Sách hiện không còn bản sẵn có để cho mượn."; return false; }

            var staff = ResolveBorrowingStaff(staffUsername);
            if (staff == null)
            {
                errorMessage = "Không xác định được cán bộ lập phiếu. Vui lòng đăng nhập lại tài khoản quản trị.";
                return false;
            }
            var copy = document.BanSaos.First(c => c.TrangThai == "Có sẵn");
            if (string.IsNullOrEmpty(reader.SoThe))
            {
                reader.SoThe = $"THE-{reader.MaSinhVien}";
                reader.NgayCapThe = DateOnly.FromDateTime(DateTime.Today);
                reader.NgayHetHanThe = DateOnly.FromDateTime(DateTime.Today.AddYears(4));
                reader.TrangThaiThe = "Hoạt động";
            }
            var loan = new PhieuMuon
            {
                MaBanDocNavigation = reader,
                MaNhanVienNavigation = staff,
                NgayMuon = borrowedAt,
                TrangThai = "Đang mượn",
                GhiChu = "Mượn tại quầy lưu thông thư viện UTC"
            };
            loan.ChiTietMuons.Add(new ChiTietMuon
            {
                MaBanSaoNavigation = copy,
                HanTra = dueDate,
                TrangThai = "Đang mượn"
            });
            copy.TrangThai = "Đang mượn";
            _dbContext.PhieuMuons.Add(loan);
            _dbContext.SaveChanges();
            transaction.Commit();
            bookTitle = document.TenTaiLieu;

            return true;
        }
        catch (Exception ex)
        {
            _dbContext.ChangeTracker.Clear();
            errorMessage = $"Không thể tạo phiếu mượn: {ex.Message}";
            return false;
        }
    }

    private NhanVien? ResolveBorrowingStaff(string? username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        var account = _dbContext.TaiKhoans.Include(a => a.MaNhanVienNavigation).SingleOrDefault(a => a.TenDangNhap == username);
        if (account == null || !BTL_LTWEB.Common.AccountStatus.IsActive(account.TrangThai) ||
            !new[] { "ADMIN", "QUANLY_BANDOC", "QUANLY_TAILIEU", "THUTHU", "ADMINSTAFF" }.Contains(account.VaiTro?.Trim().ToUpperInvariant())) return null;
        return account.MaNhanVienNavigation;
    }

    public ImportQldtViewModel GetImportPreview()
    {
        return new ImportQldtViewModel
        {
            NguonDuLieu = "Dữ liệu bạn đọc hiện có trong SQL Server",
            TepDuLieu = string.Empty,
            PreviewItems = _dbContext.BanDocs.Take(15).Select(r => new ImportQldtItemViewModel
            {
                MaSV = r.MaSinhVien,
                HoTen = r.HoTen,
                LopKhoa = r.MaLopNavigation == null ? string.Empty : r.MaLopNavigation.TenLop,
                TrangThaiKiemTra = "Đang có trong SQL Server",
                IsNew = false,
                IsValid = true
            }).ToList()
        };
    }

    public bool ProcessImport(out int updatedCount, out int insertedCount)
    {
        updatedCount = 0;
        insertedCount = 0;
        return false; // The external QLDT importer has not been implemented.
    }

    public List<CirculationTransactionItem> GetAllCirculationTransactions()
    {

        var today = DateTime.Today;
        var activeLoans = _dbContext.ChiTietMuons
            .Include(c => c.MaPhieuMuonNavigation)
                .ThenInclude(p => p.MaBanDocNavigation)
            .Include(c => c.MaBanSaoNavigation)
                .ThenInclude(b => b.MaTaiLieuNavigation)
                    .ThenInclude(tl => tl.TaiLieuTacGia)
                        .ThenInclude(tg => tg.MaTacGiaNavigation)
            .Where(c => c.NgayTra == null &&
                        c.TrangThai != "Đã trả" &&
                        c.TrangThai != "Đã đền sách mới" &&
                        c.TrangThai != "Đã đền tiền")
            .AsNoTracking()
            .ToList();

        var list = new List<CirculationTransactionItem>();
        foreach (var c in activeLoans)
        {
            var book = c.MaBanSaoNavigation?.MaTaiLieuNavigation;
            var banDoc = c.MaPhieuMuonNavigation?.MaBanDocNavigation;
            var hanTraDate = c.HanTra.ToDateTime(TimeOnly.MinValue);
            var isOverdue = today > hanTraDate || c.TrangThai == "Quá hạn";
            var daysOverdue = isOverdue ? Math.Max(1, (int)(today - hanTraDate).TotalDays) : 0;

            var authors = (book != null && book.TaiLieuTacGia.Any())
                ? string.Join(", ", book.TaiLieuTacGia.Select(tg => tg.MaTacGiaNavigation.HoTen))
                : "Nhiều tác giả";

            list.Add(new CirculationTransactionItem
            {
                MaGD = $"GD{(c.MaPhieuMuonNavigation != null ? c.MaPhieuMuonNavigation.NgayMuon : DateTime.Now):yyMMdd}-{c.MaChiTietMuon:D3}",
                MaBD = banDoc?.MaSinhVien ?? string.Empty,
                HoTen = banDoc?.HoTen ?? "Chưa xác định bạn đọc",
                MaBanSao = c.MaBanSaoNavigation?.MaVach ?? $"BS-{c.MaBanSao:D5}",
                TenSach = book?.TenTaiLieu ?? "Học liệu UTC",
                TacGia = authors,
                CoverImage = book?.AnhBia ?? "/images/covers/sach_01.jpg",
                NgayMuon = (c.MaPhieuMuonNavigation != null ? c.MaPhieuMuonNavigation.NgayMuon : DateTime.Now).ToString("dd/MM/yyyy"),
                HanTra = c.HanTra.ToString("dd/MM/yyyy"),
                TrangThai = isOverdue ? $"Quá hạn {daysOverdue} ngày" : "Còn hạn",
                IsQuaHan = isOverdue
            });
        }

        return list.OrderByDescending(x => x.IsQuaHan).ThenByDescending(x => x.NgayMuon).ToList();

    }

    public List<OverdueItemViewModel> GetAllOverdueLoans()
    {

        var all = GetAllCirculationTransactions();
        return all.Where(x => x.IsQuaHan).Select(t =>
        {
            int overdueDays = 5;
            if (t.TrangThai.Contains("ngày"))
            {
                var parts = t.TrangThai.Split(' ');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int days))
                {
                    overdueDays = days;
                }
            }

            return new OverdueItemViewModel
            {
                MaGD = t.MaGD,
                MaBD = t.MaBD,
                HoTen = t.HoTen,
                TenSach = t.TenSach,
                NgayMuon = t.NgayMuon,
                HanTra = t.HanTra,
                SoNgayQuaHan = overdueDays,
                TienPhat = 100000m
            };
        }).OrderByDescending(x => x.SoNgayQuaHan).ToList();

    }

    public FinancialSummaryViewModel GetFinancialSummary()
    {
        var receipts = GetAllFinancialReceipts(null, null);
        return new FinancialSummaryViewModel
        {
            TongThu = receipts.Sum(r => r.SoTien),
            ThuNopMuon = receipts.Sum(r => r.TienNopMuon),
            ThuDenBu = receipts.Sum(r => r.TienDenBu),
            TongPhieuThu = receipts.Count,
            RecentReceipts = receipts.Take(10).ToList()
        };
    }
    public List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? searchKeyword, string? selectedType)
    {
        var payments = _dbContext.ThanhToans.AsNoTracking().Include(t => t.MaBanDocNavigation)
            .Where(t => t.TrangThai == "Thành công").OrderByDescending(t => t.NgayThanhToan).ThenByDescending(t => t.MaThanhToan).ToList();
        var loanIds = payments.Where(t => t.MaChiTietMuon.HasValue).Select(t => t.MaChiTietMuon!.Value).ToList();
        var loans = _dbContext.ChiTietMuons.AsNoTracking().Include(c => c.MaBanSaoNavigation).ThenInclude(c => c.MaTaiLieuNavigation)
            .Include(c => c.MaPhieuMuonNavigation).Where(c => loanIds.Contains(c.MaChiTietMuon)).ToDictionary(c => c.MaChiTietMuon);
        var receipts = payments.Select(t =>
        {
            var loan = t.MaChiTietMuon.HasValue ? loans.GetValueOrDefault(t.MaChiTietMuon.Value) : null;
            return new FinancialReceiptViewModel
            {
                MaPhieuThu = $"PT{t.MaThanhToan:D6}",
                MaGD = loan == null ? "" : $"GD{loan.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{loan.MaChiTietMuon:D3}",
                MaBD = t.MaBanDocNavigation.MaSinhVien,
                NguoiNop = t.MaBanDocNavigation.HoTen,
                TenSach = loan?.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu ?? "",
                LoaiKhoanThu = t.LoaiKhoanThu ?? "Khoản thu khác",
                HinhThucXuLy = t.HinhThucXuLy ?? t.PhuongThuc,
                SoTien = t.TongTien,
                TienNopMuon = t.TienNopMuon ?? 0,
                TienDenBu = t.TienDenBu ?? 0,
                NgayThu = t.NgayThanhToan.ToString("dd/MM/yyyy HH:mm"),
                GhiChu = t.GhiChu ?? t.NoiDung ?? ""
            };
        });
        if (!string.IsNullOrWhiteSpace(searchKeyword)) receipts = receipts.Where(r => new[] { r.MaPhieuThu, r.MaBD, r.NguoiNop, r.TenSach }.Any(v => v.Contains(searchKeyword.Trim(), StringComparison.OrdinalIgnoreCase)));
        if (!string.IsNullOrWhiteSpace(selectedType) && selectedType != "Tất cả") receipts = receipts.Where(r => r.LoaiKhoanThu == selectedType);
        return receipts.ToList();
    }

    public DashboardViewModel GetDashboardStatistics()
    {
        var allLoans = GetAllCirculationTransactions();
        var overdueLoans = GetAllOverdueLoans();
        var financial = GetFinancialSummary();

        int totalBooks = _dbContext.TaiLieus.Count();
        int totalCopies = _dbContext.BanSaos.Count();
        int totalReaders = _dbContext.BanDocs.Count();
        return new DashboardViewModel
        {
            TongDauSach = totalBooks,
            TongBanSao = totalCopies,
            DangMuon = allLoans.Count,
            BanSanCo = _dbContext.BanSaos.Count(c => c.TrangThai == "Có sẵn"),
            MatChoBoiThuong = _dbContext.BanSaos.Count(c => c.TrangThai == "Đã mất" || c.TinhTrang == "Mất" || c.TinhTrang == "Hỏng"),
            LuotQuaHan = overdueLoans.Count,
            TongBanDoc = totalReaders,
            TongQuyTaiChinh = financial.TongThu,
            TienThuNopMuon = financial.ThuNopMuon,
            TienThuDenBu = financial.ThuDenBu,
            RecentReceipts = financial.RecentReceipts,
            RecentTransactions = allLoans.Take(8).Select(t => new DashboardTransactionItem
            {
                MaGD = t.MaGD,
                MaBD = t.MaBD,
                HoTen = t.HoTen,
                TenSach = t.TenSach,
                NgayMuon = t.NgayMuon,
                HanTra = t.HanTra,
                TrangThai = t.TrangThai,
                IsQuaHan = t.IsQuaHan
            }).ToList()
        };
    }
}
