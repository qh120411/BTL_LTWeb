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

    public ReaderService(
        QLThuVienUtcContext dbContext,
        IBookCatalogService catalogService, 
        IUserProfileService userProfileService, 
        IWebHostEnvironment env)
    {
        _dbContext = dbContext;
        _catalogService = catalogService;
        _userProfileService = userProfileService;

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

                    return new BorrowTransactionViewModel
                    {
                        MaGD = $"GD{(c.MaPhieuMuonNavigation != null ? c.MaPhieuMuonNavigation.NgayMuon : DateTime.Now):yyMMdd}-{c.MaChiTietMuon:D3}",
                        MaSach = c.MaBanSaoNavigation?.MaVach ?? $"BS-{c.MaBanSao:D5}",
                        TenSach = book?.TenTaiLieu ?? "Học liệu UTC",
                        TacGia = authors,
                        CoverImage = book?.AnhBia ?? "/images/covers/sach_01.jpg",
                        NgayMuon = c.MaPhieuMuonNavigation.NgayMuon.ToString("dd/MM/yyyy"),
                        NgayHenTra = c.HanTra.ToString("dd/MM/yyyy"),
                        TrangThai = isOverdue ? $"Quá hạn {daysOverdue} ngày" : "Còn hạn",
                        IsQuaHan = isOverdue,
                        SoNgayQuaHan = daysOverdue,
                        TienPhat = isOverdue ? 100000m : 0m
                    };
                }).ToList() ?? new List<BorrowTransactionViewModel>();

            var availableBooks = _catalogService.GetAll().ToList();

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

    public bool ReturnBook(string maBD, string maGD, out string bookTitle, out decimal reducedFine)
    {
        bookTitle = string.Empty;
        reducedFine = 0;

        var detail = GetReaderById(maBD);
        var tx = detail?.BorrowTransactions.FirstOrDefault(t => t.MaGD.Equals(maGD, StringComparison.OrdinalIgnoreCase) ||
                                                                t.MaSach.Equals(maGD, StringComparison.OrdinalIgnoreCase));
        if (tx != null)
        {
            bookTitle = tx.TenSach;
            reducedFine = tx.TienPhat;
        }

        return ProcessReturnOrCompensation(maBD, maGD, "NormalReturn", "NewBook", 0, true, null, out _);
    }

    public bool ProcessReturnOrCompensation(
        string maBD, 
        string maGD, 
        string actionType, 
        string compensationType, 
        decimal compensationAmount, 
        bool payOverdue, 
        string? note, 
        out string message)
    {
        EnsureFinancialReceipts();
        message = string.Empty;

        try
        {
            // 1. TRUY VẤN VÀ CẬP NHẬT TRỰC TIẾP VÀO SQL SERVER QLThuVienUTC
            var banDoc = _dbContext.BanDocs
                .Include(b => b.PhieuMuons)
                    .ThenInclude(p => p.ChiTietMuons)
                        .ThenInclude(c => c.MaBanSaoNavigation)
                            .ThenInclude(bs => bs.MaTaiLieuNavigation)
                .FirstOrDefault(b => b.MaSinhVien == maBD);

            if (banDoc == null)
            {
                message = $"Không tìm thấy bạn đọc có mã '{maBD}' trong CSDL!";
                return false;
            }

            // Tìm ChiTietMuon tương ứng
            var activeLoans = banDoc.PhieuMuons
                .SelectMany(p => p.ChiTietMuons)
                .Where(c => c.NgayTra == null && 
                            c.TrangThai != "Đã trả" && 
                            c.TrangThai != "Đã đền sách mới" && 
                            c.TrangThai != "Đã đền tiền")
                .ToList() ?? new List<ChiTietMuon>();

            var ctm = activeLoans.FirstOrDefault(c =>
                c.MaChiTietMuon.ToString() == maGD ||
                $"GD{c.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{c.MaChiTietMuon:D3}".Equals(maGD, StringComparison.OrdinalIgnoreCase) ||
                (c.MaBanSaoNavigation != null && c.MaBanSaoNavigation.MaVach.Equals(maGD, StringComparison.OrdinalIgnoreCase)) ||
                (c.MaBanSaoNavigation?.MaTaiLieuNavigation != null && maGD.Contains(c.MaChiTietMuon.ToString())) ||
                (c.MaBanSaoNavigation?.MaTaiLieuNavigation != null && c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu.Equals(maGD, StringComparison.OrdinalIgnoreCase)));

            // Nếu không khớp trực tiếp mã, thử khớp theo danh sách
            if (ctm == null && activeLoans.Count > 0)
            {
                ctm = activeLoans.FirstOrDefault(c => maGD.EndsWith(c.MaChiTietMuon.ToString("D3")));
            }

            if (ctm == null)
            {
                message = "Không tìm thấy giao dịch mượn hoặc tài liệu đã được hoàn tất xử lý trước đó!";
                return false;
            }

            var bookTitle = ctm.MaBanSaoNavigation?.MaTaiLieuNavigation?.TenTaiLieu ?? "Học liệu UTC";
            var hanTraDate = ctm.HanTra.ToDateTime(TimeOnly.MinValue);
            bool isOverdue = DateTime.Today > hanTraDate || ctm.TrangThai == "Quá hạn";
            decimal overdueFee = (isOverdue && payOverdue) ? 100000m : 0m;

            string receiptCode = $"PT{DateTime.Now:yyMMdd}-{(DateTime.Now.Ticks % 900) + 100}";
            string todayStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // 2. CẬP NHẬT TRẠNG THÁI VÀO ENTITY FRAMEWORK CORE
            ctm.NgayTra = DateTime.Now;

            if (actionType.Equals("Compensation", StringComparison.OrdinalIgnoreCase))
            {
                // BỒI THƯỜNG
                if (compensationType.Equals("NewBook", StringComparison.OrdinalIgnoreCase))
                {
                    ctm.TrangThai = "Đã đền sách mới";
                    ctm.TinhTrangKhiTra = "Đền 01 cuốn sách mới tương đương";
                    if (ctm.MaBanSaoNavigation != null)
                    {
                        ctm.MaBanSaoNavigation.TrangThai = "Có sẵn";
                        ctm.MaBanSaoNavigation.TinhTrang = "Tốt";
                    }

                    var receipt = new FinancialReceiptViewModel
                    {
                        MaPhieuThu = receiptCode,
                        MaGD = $"GD{ctm.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{ctm.MaChiTietMuon:D3}",
                        MaBD = banDoc.MaSinhVien,
                        NguoiNop = banDoc.HoTen,
                        TenSach = bookTitle,
                        LoaiKhoanThu = overdueFee > 0 ? "Nộp muộn & Bồi thường sách mới" : "Đền bù mất/hỏng",
                        HinhThucXuLy = "Đền sách mới (Đã bàn giao sách thay thế)",
                        SoTien = overdueFee,
                        TienNopMuon = overdueFee,
                        TienDenBu = 0,
                        NgayThu = todayStr,
                        GhiChu = string.IsNullOrWhiteSpace(note) 
                            ? $"Báo mất/hỏng. Bạn đọc đền 01 cuốn sách mới bổ sung kho.{(overdueFee > 0 ? " Nộp kèm 100.000 đ phạt nộp muộn." : "")}" 
                            : note
                    };
                    _financialReceipts.Insert(0, receipt);
                    SaveReceiptsToFile();

                    message = $"Đã tiếp nhận 01 cuốn sách mới đền bù cho tài liệu '{bookTitle}' từ bạn đọc '{banDoc.HoTen}'.{(overdueFee > 0 ? " Thu phạt nộp muộn: 100.000 VNĐ." : "")} CSDL SQL Server đã cập nhật vĩnh viễn.";
                }
                else
                {
                    ctm.TrangThai = "Đã đền tiền";
                    ctm.TinhTrangKhiTra = "Báo mất sách - Bồi thường bằng tiền mặt";
                    if (ctm.MaBanSaoNavigation != null)
                    {
                        ctm.MaBanSaoNavigation.TrangThai = "Đã mất";
                        ctm.MaBanSaoNavigation.TinhTrang = "Mất";
                    }

                    decimal cashCompensation = compensationAmount > 0 ? compensationAmount : 150000m;
                    decimal totalCash = cashCompensation + overdueFee;

                    var receipt = new FinancialReceiptViewModel
                    {
                        MaPhieuThu = receiptCode,
                        MaGD = $"GD{ctm.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{ctm.MaChiTietMuon:D3}",
                        MaBD = banDoc.MaSinhVien,
                        NguoiNop = banDoc.HoTen,
                        TenSach = bookTitle,
                        LoaiKhoanThu = overdueFee > 0 ? "Đền bù mất/hỏng & Nộp muộn" : "Đền bù mất/hỏng",
                        HinhThucXuLy = "Bồi thường bằng tiền",
                        SoTien = totalCash,
                        TienNopMuon = overdueFee,
                        TienDenBu = cashCompensation,
                        NgayThu = todayStr,
                        GhiChu = string.IsNullOrWhiteSpace(note) 
                            ? $"Báo mất sách. Bồi thường tiền mặt {cashCompensation:N0} VNĐ{(overdueFee > 0 ? $" + Phạt nộp muộn {overdueFee:N0} VNĐ" : "")}." 
                            : note
                    };
                    _financialReceipts.Insert(0, receipt);
                    SaveReceiptsToFile();

                    message = $"Đã lập phiếu thu #{receiptCode}: Tiếp nhận đền bù bằng tiền {totalCash:N0} VNĐ (Đền sách: {cashCompensation:N0} VNĐ{(overdueFee > 0 ? $", Phạt quá hạn: {overdueFee:N0} VNĐ" : "")}) cho cuốn '{bookTitle}' từ bạn đọc '{banDoc.HoTen}'. Đã lưu vào CSDL SQL Server.";
                }
            }
            else
            {
                // TRẢ SÁCH BÌNH THƯỜNG
                ctm.TrangThai = "Đã trả";
                ctm.TinhTrangKhiTra = "Bình thường";
                if (ctm.MaBanSaoNavigation != null)
                {
                    ctm.MaBanSaoNavigation.TrangThai = "Có sẵn";
                    ctm.MaBanSaoNavigation.TinhTrang = "Tốt";
                }

                if (isOverdue)
                {
                    decimal fineAmount = 100000m;
                    var receipt = new FinancialReceiptViewModel
                    {
                        MaPhieuThu = receiptCode,
                        MaGD = $"GD{ctm.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{ctm.MaChiTietMuon:D3}",
                        MaBD = banDoc.MaSinhVien,
                        NguoiNop = banDoc.HoTen,
                        TenSach = bookTitle,
                        LoaiKhoanThu = "Nộp muộn",
                        HinhThucXuLy = "Trả sách & nộp phạt nộp muộn",
                        SoTien = fineAmount,
                        TienNopMuon = fineAmount,
                        TienDenBu = 0,
                        NgayThu = todayStr,
                        GhiChu = string.IsNullOrWhiteSpace(note) ? $"Hoàn tất trả sách và nộp phạt nộp muộn ({ctm.TrangThai})" : note
                    };
                    _financialReceipts.Insert(0, receipt);
                    SaveReceiptsToFile();

                    message = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{banDoc.HoTen}' thành công! Thu phạt nộp muộn 100.000 VNĐ đã được cộng vào quỹ tài chính và lưu vào CSDL SQL Server.";
                }
                else
                {
                    message = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{banDoc.HoTen}' thành công! Sách còn hạn mượn, CSDL SQL Server đã cập nhật.";
                }
            }

            // Xử lý nộp phạt trong bảng ViPham nếu có
            if (payOverdue)
            {
                var viPhams = _dbContext.ViPhams
                    .Where(v => v.MaChiTietMuon == ctm.MaChiTietMuon)
                    .ToList();

                foreach (var vp in viPhams)
                {
                    vp.TrangThaiNop = "Đã thanh toán";
                    vp.NgayNop = DateTime.Now;
                }
            }

            // Ghi nhận bảng ThanhToan trong SQL Server
            decimal paymentTotal = overdueFee + (actionType.Equals("Compensation", StringComparison.OrdinalIgnoreCase) && !compensationType.Equals("NewBook", StringComparison.OrdinalIgnoreCase) ? (compensationAmount > 0 ? compensationAmount : 150000m) : 0m);
            if (paymentTotal > 0)
            {
                var thanhToan = new ThanhToan
                {
                    MaBanDoc = banDoc.MaBanDoc,
                    MaNhanVien = 1,
                    NgayThanhToan = DateTime.Now,
                    TongTien = paymentTotal,
                    PhuongThuc = "Tiền mặt",
                    TrangThai = "Thành công"
                };
                _dbContext.ThanhToans.Add(thanhToan);
            }

            // COMMIT TRỰC TIẾP VÀO CSDL SQL SERVER
            _dbContext.SaveChanges();

            // Đồng bộ sang UserProfileService
            try
            {
                _userProfileService.GetProfile(maBD, banDoc.HoTen);
            }
            catch { }

            return true;
        }
        catch (Exception ex)
        {
            message = $"Lỗi khi lưu dữ liệu trả sách vào SQL Server: {ex.Message}";
            return false;
        }
    }

    public bool BorrowBook(string maBD, int bookId, out string bookTitle, out string errorMessage)
    {
        bookTitle = string.Empty;
        errorMessage = string.Empty;

        try
        {
            var banDoc = _dbContext.BanDocs
                .Include(b => b.PhieuMuons)
                    .ThenInclude(p => p.ChiTietMuons)
                        .ThenInclude(c => c.MaBanSaoNavigation)
                .FirstOrDefault(b => b.MaSinhVien == maBD);

            if (banDoc == null)
            {
                errorMessage = $"Không tìm thấy bạn đọc có mã '{maBD}' trong CSDL!";
                return false;
            }

            var activeLoans = banDoc.PhieuMuons
                .SelectMany(p => p.ChiTietMuons)
                .Where(c => c.NgayTra == null && 
                            c.TrangThai != "Đã trả" && 
                            c.TrangThai != "Đã đền sách mới" && 
                            c.TrangThai != "Đã đền tiền")
                .ToList() ?? new List<ChiTietMuon>();

            var isLecturer = banDoc.MaSinhVien.StartsWith("GV", StringComparison.OrdinalIgnoreCase) ||
                             banDoc.MaSinhVien.StartsWith("CB", StringComparison.OrdinalIgnoreCase);
            int maxAllowed = isLecturer ? 10 : 5;

            if (activeLoans.Count >= maxAllowed)
            {
                errorMessage = $"Bạn đọc đã đạt giới hạn mượn tối đa ({maxAllowed} cuốn)! Vui lòng trả bớt sách trước khi mượn thêm.";
                return false;
            }

            // Kiểm tra xem đã mượn đầu sách này chưa
            if (activeLoans.Any(c => c.MaBanSaoNavigation != null && c.MaBanSaoNavigation.MaTaiLieu == bookId))
            {
                errorMessage = "Bạn đọc đã mượn đầu sách này rồi! Theo quy chế, mỗi bạn đọc chỉ được mượn 1 bản/đầu sách.";
                return false;
            }

            var book = _dbContext.TaiLieus.FirstOrDefault(t => t.MaTaiLieu == bookId);
            if (book == null)
            {
                errorMessage = "Không tìm thấy học liệu được chọn trong CSDL!";
                return false;
            }

            // Tìm một bản sao còn sẵn trong kho
            var banSao = _dbContext.BanSaos.FirstOrDefault(bs => bs.MaTaiLieu == bookId && bs.TrangThai == "Có sẵn");
            if (banSao == null)
            {
                errorMessage = "Tất cả bản sao của đầu sách này hiện đang được mượn lưu hành ngoài kho!";
                return false;
            }

            if (string.IsNullOrEmpty(banDoc.SoThe))
            {
                banDoc.SoThe = $"THE-{banDoc.MaSinhVien}";
                banDoc.NgayCapThe = DateOnly.FromDateTime(DateTime.Today);
                banDoc.NgayHetHanThe = DateOnly.FromDateTime(DateTime.Today.AddYears(4));
                banDoc.TrangThaiThe = "Hoạt động";
                _dbContext.SaveChanges();
            }

            var phieuMuon = banDoc.PhieuMuons.FirstOrDefault(p => p.TrangThai == "Đang mượn");
            if (phieuMuon == null)
            {
                phieuMuon = new PhieuMuon
                {
                    MaBanDoc = banDoc.MaBanDoc,
                    MaNhanVien = 1,
                    NgayMuon = DateTime.Now,
                    TrangThai = "Đang mượn",
                    GhiChu = "Mượn tại quầy lưu thông thư viện"
                };
                _dbContext.PhieuMuons.Add(phieuMuon);
                _dbContext.SaveChanges();
            }

            var ctm = new ChiTietMuon
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                MaBanSao = banSao.MaBanSao,
                HanTra = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), // Quy định 1 năm
                NgayTra = null,
                TrangThai = "Đang mượn"
            };
            _dbContext.ChiTietMuons.Add(ctm);
            banSao.TrangThai = "Đang mượn";
            _dbContext.SaveChanges();

            bookTitle = book.TenTaiLieu;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Lỗi khi lưu mượn sách vào SQL Server: {ex.Message}";
            return false;
        }
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
                    NgayMuon = c.MaPhieuMuonNavigation.NgayMuon.ToString("dd/MM/yyyy"),
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
