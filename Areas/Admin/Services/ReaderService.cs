using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Services;

public class ReaderRecord
{
    public string MaBD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string LopKhoa { get; set; } = string.Empty;
    public string Nhom { get; set; } = string.Empty;
    public string TrangThaiHocTap { get; set; } = string.Empty;
    public string QuyenSuDung { get; set; } = string.Empty;
    public string GioiHanSach { get; set; } = "5 cuốn";
    public string HieuLucQuyen { get; set; } = "2028-06-30";
    public int SoSachDangMuon => BorrowTransactions.Count;
    public int SoSachQuaHan => BorrowTransactions.Count(b => b.IsQuaHan);
    public decimal TongTienPhat => BorrowTransactions.Where(b => b.IsQuaHan).Sum(b => b.TienPhat);
    public List<BorrowTransactionViewModel> BorrowTransactions { get; set; } = new();
}

public class ReaderService : IReaderService
{
    private static readonly List<FinancialReceiptViewModel> _financialReceipts = new();
    private static readonly object _initLock = new();
    private static bool _receiptsLoaded = false;

    private readonly QLThuVienUtcContext _dbContext;
    private readonly IBookCatalogService _catalogService;
    private readonly IUserProfileService _userProfileService;
    private readonly string _receiptsFilePath;
    private readonly TimeProvider _timeProvider;

    public ReaderService(
        QLThuVienUtcContext dbContext,
        IBookCatalogService catalogService, 
        IUserProfileService userProfileService, 
        IWebHostEnvironment env,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _catalogService = catalogService;
        _userProfileService = userProfileService;
        _timeProvider = timeProvider ?? TimeProvider.System;

        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _receiptsFilePath = Path.Combine(dataDir, "financial_receipts.json");

        EnsureFinancialReceipts();
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

    private void EnsureFinancialReceipts()
    {
        if (_receiptsLoaded) return;
        lock (_initLock)
        {
            if (_receiptsLoaded) return;
            try
            {
                if (File.Exists(_receiptsFilePath))
                {
                    var json = File.ReadAllText(_receiptsFilePath, Encoding.UTF8);
                    var list = JsonSerializer.Deserialize<List<FinancialReceiptViewModel>>(json);
                    if (list != null && list.Count > 0)
                    {
                        _financialReceipts.Clear();
                        _financialReceipts.AddRange(list);
                        _receiptsLoaded = true;
                        return;
                    }
                }
            }
            catch { }

            if (_financialReceipts.Count == 0)
            {
                _financialReceipts.Add(new FinancialReceiptViewModel
                {
                    MaPhieuThu = "PT261015-001",
                    MaGD = "GD250810-088",
                    MaBD = "242630923",
                    NguoiNop = "Đỗ Văn Đạt",
                    TenSach = "Giáo trình Lập trình Hướng đối tượng Java",
                    LoaiKhoanThu = "Nộp muộn",
                    HinhThucXuLy = "Trả sách & nộp phạt nộp muộn",
                    SoTien = 100000m,
                    TienNopMuon = 100000m,
                    TienDenBu = 0,
                    NgayThu = "15/10/2026 09:30",
                    GhiChu = "Nộp phạt quá hạn 18 ngày khi trả sách tại quầy"
                });

                _financialReceipts.Add(new FinancialReceiptViewModel
                {
                    MaPhieuThu = "PT261022-002",
                    MaGD = "GD250912-092",
                    MaBD = "242630899",
                    NguoiNop = "Nguyễn Văn An",
                    TenSach = "Giáo trình Truyền số liệu và Mạng máy tính",
                    LoaiKhoanThu = "Đền bù mất/hỏng",
                    HinhThucXuLy = "Bồi thường bằng tiền",
                    SoTien = 150000m,
                    TienNopMuon = 0,
                    TienDenBu = 150000m,
                    NgayThu = "22/10/2026 14:15",
                    GhiChu = "Báo mất sách khi đi thực tập. Đền bù bằng tiền mặt theo giá bìa"
                });

                _financialReceipts.Add(new FinancialReceiptViewModel
                {
                    MaPhieuThu = "PT261030-003",
                    MaGD = "GD250720-095",
                    MaBD = "242630909",
                    NguoiNop = "Trương Đức Bình",
                    TenSach = "Cơ sở Dữ liệu Nâng cao",
                    LoaiKhoanThu = "Đền bù mất/hỏng",
                    HinhThucXuLy = "Đền sách mới (Đã bàn giao sách thay thế)",
                    SoTien = 0,
                    TienNopMuon = 0,
                    TienDenBu = 0,
                    NgayThu = "30/10/2026 16:45",
                    GhiChu = "Làm ướt sách. Bạn đọc tự mua và nộp lại 01 cuốn sách mới tương đương"
                });

                SaveReceiptsToFile();
            }

            _receiptsLoaded = true;
        }
    }

    private void SaveReceiptsToFile()
    {
        try
        {
            lock (_initLock)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };
                var json = JsonSerializer.Serialize(_financialReceipts, options);
                File.WriteAllText(_receiptsFilePath, json, Encoding.UTF8);
            }
        }
        catch { }
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
        EnsureFinancialReceipts();

        try
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
                    ? $"{b.MaLopNavigation.TenLop} · {b.MaLopNavigation.KhoaHoc ?? "CLC CNTT"}"
                    : "K65.CLCCNTT1 · CLC CNTT Việt - Anh";

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
        catch
        {
            return new ReaderIndexViewModel
            {
                SearchKeyword = searchKeyword,
                SelectedGroup = selectedGroup ?? "Tất cả nhóm",
                Readers = new List<ReaderListItemViewModel>()
            };
        }
    }

    public ReaderDetailViewModel? GetReaderById(string id)
    {
        EnsureFinancialReceipts();

        try
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
            try
            {
                CatalogSqlBridge.RefreshAvailability(availableBooks, _dbContext);
            }
            catch { }

            string lopKhoa = b.MaLopNavigation != null
                ? $"{b.MaLopNavigation.TenLop} · {b.MaLopNavigation.KhoaHoc ?? "CLC CNTT"}"
                : "K65.CLCCNTT1 · CLC CNTT Việt - Anh";

            return new ReaderDetailViewModel
            {
                MaBD = b.MaSinhVien,
                HoTen = b.HoTen,
                LopKhoa = lopKhoa,
                TrangThaiHocTap = isLecturer ? "Đang công tác" : "Đang học",
                Nhom = isLecturer ? "Giảng viên" : "Sinh viên",
                QuyenSuDung = b.TrangThai == "Hoạt động" ? "Hoạt động" : "Đang khóa",
                GioiHanSach = isLecturer ? "10 cuốn" : "5 cuốn",
                HieuLucQuyen = "2028-06-30",
                BorrowTransactions = activeLoans,
                AvailableBooks = availableBooks
            };
        }
        catch
        {
            return null;
        }
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
        EnsureFinancialReceipts();
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
            var staff = string.IsNullOrWhiteSpace(staffUsername)
                ? _dbContext.NhanViens.Find(loan.MaPhieuMuonNavigation.MaNhanVien)
                : ResolveBorrowingStaff(staffUsername);
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
            if (total > 0)
            {
                _dbContext.ThanhToans.Add(new ThanhToan
                {
                    MaBanDoc = reader.MaBanDoc, MaNhanVienNavigation = staff,
                    NgayThanhToan = now, TongTien = total, PhuongThuc = "Tiền mặt",
                    TrangThai = "Thành công", NoiDung = $"Thu phí lưu thông: {transactionCode} ({book.TenTaiLieu})"
                });
            }
            _dbContext.SaveChanges();
            transaction.Commit();

            // Append the receipt only after SQL commits, so a failed return cannot create a phantom receipt.
            if (isCompensation || total > 0)
            {
                var receipt = new FinancialReceiptViewModel
                {
                    MaPhieuThu = $"PT{now:yyMMdd}-{Guid.NewGuid().ToString("N")[..8]}",
                    MaGD = transactionCode, MaBD = reader.MaSinhVien, NguoiNop = reader.HoTen,
                    TenSach = book.TenTaiLieu, LoaiKhoanThu = isCompensation ? "Đền bù mất/hỏng" : "Nộp muộn",
                    HinhThucXuLy = isCompensation ? (isCash ? "Đền tiền" : "Đền sách mới") : "Trả sách quá hạn",
                    SoTien = total, TienNopMuon = overdueFee, TienDenBu = compensationFee,
                    NgayThu = now.ToString("dd/MM/yyyy HH:mm"), GhiChu = note?.Trim() ?? ""
                };
                lock (_initLock) { _financialReceipts.Insert(0, receipt); SaveReceiptsToFile(); }
            }
            try { CatalogSqlBridge.SyncCopies(_catalogService, _dbContext, book); } catch { }
            try { _userProfileService.GetProfile(reader.MaSinhVien, reader.HoTen); } catch { }
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

            // Catalog IDs are independent of SQL identity keys; match the actual title/ISBN.
            var hasIsbn = !string.IsNullOrWhiteSpace(catalogBook.ISBN);
            var document = _dbContext.TaiLieus.Include(d => d.BanSaos).FirstOrDefault(d =>
                d.TenTaiLieu == catalogBook.Title ||
                (hasIsbn && d.Isbn == catalogBook.ISBN));
            if (document != null && activeLoans.Any(c => c.MaBanSaoNavigation.MaTaiLieu == document.MaTaiLieu))
            {
                errorMessage = "Bạn đọc đã mượn đầu sách này rồi!";
                return false;
            }
            if ((document != null && !document.BanSaos.Any(c => c.TrangThai == "Có sẵn")) ||
                (document == null && (catalogBook.AvailableCopies <= 0 || catalogBook.TotalCopies <= 0)))
            {
                errorMessage = "Sách hiện không còn bản sẵn có để cho mượn.";
                return false;
            }

            var staff = ResolveBorrowingStaff(staffUsername);
            if (staff == null)
            {
                errorMessage = "Không xác định được cán bộ lập phiếu. Vui lòng đăng nhập lại tài khoản quản trị.";
                return false;
            }
            if (document == null)
            {
                document = CreateCatalogDocument(catalogBook);
                _dbContext.TaiLieus.Add(document);
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

            // SQL is authoritative once the loan has been committed.
            try { CatalogSqlBridge.SyncCopies(_catalogService, _dbContext, document); } catch { }
            try { _userProfileService.GetProfile(reader.MaSinhVien, reader.HoTen); } catch { }
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
        if (string.IsNullOrWhiteSpace(username))
            return _dbContext.NhanViens.FirstOrDefault(n => n.TrangThai == "Hoạt động");

        var account = _dbContext.TaiKhoans.Include(a => a.MaNhanVienNavigation)
            .FirstOrDefault(a => a.TenDangNhap == username);
        var role = account?.VaiTro?.ToUpperInvariant();
        if (account == null || !new[] { "ADMIN", "QUANLY_BANDOC", "QUANLY_TAILIEU", "THUTHU", "ADMINSTAFF" }.Contains(role))
            return null;
        if (account.MaNhanVienNavigation == null)
        {
            account.MaNhanVienNavigation = new NhanVien
            {
                MaSoNhanVien = $"NV{account.MaTaiKhoan:D6}",
                HoTen = role == "ADMIN" ? "Quản trị viên thư viện" : account.TenDangNhap,
                Email = account.EmailDangNhap,
                ChucVu = role == "ADMIN" ? "Quản trị viên" : "Thủ thư",
                TrangThai = "Hoạt động"
            };
        }
        return account.MaNhanVienNavigation;
    }

    private TaiLieu CreateCatalogDocument(BookItem source)
    {
        var publisher = string.IsNullOrWhiteSpace(source.Publisher) ? null :
            _dbContext.NhaXuatBans.FirstOrDefault(p => p.TenNhaXuatBan == source.Publisher) ??
            new NhaXuatBan { TenNhaXuatBan = source.Publisher };
        var document = new TaiLieu
        {
            TenTaiLieu = source.Title,
            Isbn = string.IsNullOrWhiteSpace(source.ISBN) ? null : source.ISBN,
            MaNhaXuatBanNavigation = publisher,
            NgonNgu = source.LanguageName,
            DinhDang = "Sách in",
            NamXuatBan = (short)Math.Clamp(source.Year, 0, short.MaxValue),
            SoTrang = source.Pages,
            MoTa = source.Description,
            AnhBia = source.CoverImage,
            NgayTao = DateTime.Now
        };
        foreach (var name in (source.Authors ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct())
        {
            var author = _dbContext.TacGia.Local.FirstOrDefault(a => a.HoTen == name) ??
                         _dbContext.TacGia.FirstOrDefault(a => a.HoTen == name) ?? new TacGium { HoTen = name };
            document.TaiLieuTacGia.Add(new TaiLieuTacGium { MaTacGiaNavigation = author, VaiTroTacGia = "Tác giả" });
        }
        // Register this catalog title only when the first confirmed loan needs it.
        for (var i = 1; i <= source.TotalCopies; i++)
        {
            document.BanSaos.Add(new BanSao
            {
                MaVach = $"UTC-{source.Id:D2}-{i:D2}",
                ViTriKe = source.Shelf,
                TinhTrang = "Tốt",
                TrangThai = i <= source.AvailableCopies ? "Có sẵn" : "Đang mượn",
                NgayNhap = DateOnly.FromDateTime(DateTime.Today)
            });
        }
        return document;
    }

    public ImportQldtViewModel GetImportPreview()
    {
        return new ImportQldtViewModel
        {
            NguonDuLieu = "Cổng thông tin Đào tạo QLĐT - Đại học GTVT",
            TepDuLieu = "DanhSach_SinhVien_K65_CLCCNTT1.xlsx",
            PreviewItems = _dbContext.BanDocs.Take(15).Select(r => new ImportQldtItemViewModel
            {
                MaSV = r.MaSinhVien,
                HoTen = r.HoTen,
                LopKhoa = "K65.CLCCNTT1 · CLC CNTT Việt - Anh",
                TrangThaiKiemTra = "Đã đồng bộ thành công",
                IsNew = false,
                IsValid = true
            }).ToList()
        };
    }

    public bool ProcessImport(out int updatedCount, out int insertedCount)
    {
        updatedCount = _dbContext.BanDocs.Count();
        insertedCount = 0;
        return true;
    }

    public List<CirculationTransactionItem> GetAllCirculationTransactions()
    {
        try
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
                    MaBD = banDoc?.MaSinhVien ?? "242630923",
                    HoTen = banDoc?.HoTen ?? "Đỗ Văn Đạt",
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
        catch
        {
            return new List<CirculationTransactionItem>();
        }
    }

    public Dictionary<string, List<string>> GetReaderBorrowedTitlesMap()
    {
        try
        {
            var banDocs = _dbContext.BanDocs
                .Include(b => b.PhieuMuons)
                    .ThenInclude(p => p.ChiTietMuons)
                        .ThenInclude(c => c.MaBanSaoNavigation)
                            .ThenInclude(bs => bs.MaTaiLieuNavigation)
                .AsNoTracking()
                .ToList();

            var map = new Dictionary<string, List<string>>();
            foreach (var b in banDocs)
            {
                var titles = b.PhieuMuons
                    .SelectMany(p => p.ChiTietMuons)
                    .Where(c => c.NgayTra == null && 
                                c.TrangThai != "Đã trả" && 
                                c.TrangThai != "Đã đền sách mới" && 
                                c.TrangThai != "Đã đền tiền")
                    .Select(c => c.MaBanSaoNavigation?.MaTaiLieuNavigation?.TenTaiLieu ?? "Học liệu")
                    .ToList() ?? new List<string>();

                map[b.MaSinhVien] = titles;
            }
            return map;
        }
        catch
        {
            return new Dictionary<string, List<string>>();
        }
    }

    public List<OverdueItemViewModel> GetAllOverdueLoans()
    {
        try
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
        catch
        {
            return new List<OverdueItemViewModel>();
        }
    }

    public FinancialSummaryViewModel GetFinancialSummary()
    {
        EnsureFinancialReceipts();
        return new FinancialSummaryViewModel
        {
            TongThu = _financialReceipts.Sum(r => r.SoTien),
            ThuNopMuon = _financialReceipts.Sum(r => r.TienNopMuon),
            ThuDenBu = _financialReceipts.Sum(r => r.TienDenBu),
            TongPhieuThu = _financialReceipts.Count,
            RecentReceipts = _financialReceipts.Take(10).ToList()
        };
    }

    public List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? searchKeyword, string? selectedType)
    {
        EnsureFinancialReceipts();
        var query = _financialReceipts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var kw = searchKeyword.Trim().ToLower();
            query = query.Where(r => r.MaPhieuThu.ToLower().Contains(kw) ||
                                     r.MaBD.ToLower().Contains(kw) ||
                                     r.NguoiNop.ToLower().Contains(kw) ||
                                     r.TenSach.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(selectedType) && selectedType != "Tất cả")
        {
            if (selectedType == "Nộp muộn")
            {
                query = query.Where(r => r.LoaiKhoanThu.Contains("Nộp muộn"));
            }
            else if (selectedType == "Đền bù mất/hỏng")
            {
                query = query.Where(r => r.LoaiKhoanThu.Contains("Đền bù"));
            }
        }

        return query.OrderByDescending(r => r.NgayThu).ToList();
    }

    public DashboardViewModel GetDashboardStatistics()
    {
        EnsureFinancialReceipts();
        var allLoans = GetAllCirculationTransactions();
        var overdueLoans = GetAllOverdueLoans();
        var financial = GetFinancialSummary();

        int totalBooks = 30;
        int totalCopies = 120;
        int totalReaders = 49;

        try
        {
            totalBooks = _dbContext.TaiLieus.Count();
            totalCopies = _dbContext.BanSaos.Count();
            totalReaders = _dbContext.BanDocs.Count();
        }
        catch { }

        return new DashboardViewModel
        {
            TongDauSach = totalBooks,
            TongBanSao = totalCopies,
            DangMuon = allLoans.Count,
            BanSanCo = Math.Max(0, totalCopies - allLoans.Count),
            MatChoBoiThuong = 0,
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
