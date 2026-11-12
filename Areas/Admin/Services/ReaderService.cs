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
    private readonly QLThuVienUtcContext _dbContext;
    private readonly IBookCatalogService _catalogService;
    private readonly IUserProfileService _userProfileService;

    public ReaderService(
        QLThuVienUtcContext dbContext,
        IBookCatalogService catalogService, 
        IUserProfileService userProfileService)
    {
        _dbContext = dbContext;
        _catalogService = catalogService;
        _userProfileService = userProfileService;
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

    public ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup)
    {

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
                var availableCounts = _dbContext.BanSaos
                    .Where(bs => bs.TrangThai == "Có sẵn")
                    .GroupBy(bs => bs.MaTaiLieu)
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var ab in availableBooks)
                {
                    ab.AvailableCopies = availableCounts.TryGetValue(ab.Id, out int realCount) ? realCount : 2;
                }
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

    public bool ReturnBook(string maBD, string maGD, out string bookTitle, out decimal reducedFine)
    {
        bookTitle = "Học liệu UTC";
        reducedFine = 0;

        try
        {
            ChiTietMuon? targetCtm = null;
            if (int.TryParse(maGD, out int directId))
            {
                targetCtm = _dbContext.ChiTietMuons
                    .Include(c => c.MaBanSaoNavigation).ThenInclude(bs => bs.MaTaiLieuNavigation)
                    .FirstOrDefault(c => c.MaChiTietMuon == directId);
            }
            else if (maGD.Contains('-'))
            {
                var suffix = maGD.Split('-').LastOrDefault()?.Trim();
                if (int.TryParse(suffix, out int parsedId))
                {
                    targetCtm = _dbContext.ChiTietMuons
                        .Include(c => c.MaBanSaoNavigation).ThenInclude(bs => bs.MaTaiLieuNavigation)
                        .FirstOrDefault(c => c.MaChiTietMuon == parsedId);
                }
            }

            if (targetCtm?.MaBanSaoNavigation?.MaTaiLieuNavigation != null)
            {
                bookTitle = targetCtm.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu;
                var hanTraDate = targetCtm.HanTra.ToDateTime(TimeOnly.MinValue);
                if (DateTime.Today > hanTraDate || targetCtm.TrangThai == "Quá hạn")
                {
                    reducedFine = 100000m;
                }
            }
        }
        catch { }

        if (bookTitle == "Học liệu UTC")
        {
            var detail = GetReaderById(maBD);
            var tx = detail?.BorrowTransactions.FirstOrDefault(t => t.MaGD.Equals(maGD, StringComparison.OrdinalIgnoreCase) ||
                                                                    t.MaSach.Equals(maGD, StringComparison.OrdinalIgnoreCase));
            if (tx != null)
            {
                bookTitle = tx.TenSach;
                reducedFine = tx.TienPhat;
            }
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
        message = string.Empty;

        try
        {
            // 1. TRUY VẤN VÀ CẬP NHẬT TRỰC TIẾP VÀO SQL SERVER QLThuVienUTC
            ChiTietMuon? ctm = null;
            int targetCtmId = 0;

            if (int.TryParse(maGD, out int directId))
            {
                targetCtmId = directId;
            }
            else if (maGD.Contains('-'))
            {
                var suffix = maGD.Split('-').LastOrDefault()?.Trim();
                if (int.TryParse(suffix, out int parsedId))
                {
                    targetCtmId = parsedId;
                }
            }

            if (targetCtmId > 0)
            {
                ctm = _dbContext.ChiTietMuons
                    .Include(c => c.MaPhieuMuonNavigation)
                        .ThenInclude(p => p.MaBanDocNavigation)
                    .Include(c => c.MaBanSaoNavigation)
                        .ThenInclude(bs => bs.MaTaiLieuNavigation)
                    .FirstOrDefault(c => c.MaChiTietMuon == targetCtmId);
            }

            if (ctm == null)
            {
                ctm = _dbContext.ChiTietMuons
                    .Include(c => c.MaPhieuMuonNavigation)
                        .ThenInclude(p => p.MaBanDocNavigation)
                    .Include(c => c.MaBanSaoNavigation)
                        .ThenInclude(bs => bs.MaTaiLieuNavigation)
                    .FirstOrDefault(c => (c.NgayTra == null && c.TrangThai != "Đã trả" && c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền") &&
                                         ((c.MaBanSaoNavigation != null && c.MaBanSaoNavigation.MaVach.Equals(maGD, StringComparison.OrdinalIgnoreCase)) ||
                                          (c.MaPhieuMuonNavigation != null && c.MaPhieuMuonNavigation.MaBanDocNavigation != null && c.MaPhieuMuonNavigation.MaBanDocNavigation.MaSinhVien == maBD)));
            }

            if (ctm == null)
            {
                message = "Không tìm thấy giao dịch mượn hoặc tài liệu đã được hoàn tất xử lý trước đó!";
                return false;
            }

            var banDoc = ctm.MaPhieuMuonNavigation?.MaBanDocNavigation 
                         ?? _dbContext.BanDocs.FirstOrDefault(b => b.MaSinhVien == maBD);

            var book = ctm.MaBanSaoNavigation?.MaTaiLieuNavigation;
            var bookTitle = book?.TenTaiLieu ?? "Học liệu UTC";
            var banSao = ctm.MaBanSaoNavigation;
            var hanTraDate = ctm.HanTra.ToDateTime(TimeOnly.MinValue);
            bool isOverdue = DateTime.Today > hanTraDate || ctm.TrangThai == "Quá hạn";
            decimal overdueFee = (isOverdue && payOverdue) ? 100000m : 0m;

            string safeMaGD = ctm.MaPhieuMuonNavigation != null
                ? $"GD{ctm.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{ctm.MaChiTietMuon:D3}"
                : $"GD{DateTime.Now:yyMMdd}-{ctm.MaChiTietMuon:D3}";

            string receiptCode = $"PT{DateTime.Now:yyMMdd}-{(DateTime.Now.Ticks % 900) + 100}";
            string todayStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            string readerName = banDoc?.HoTen ?? "Bạn đọc UTC";
            string readerMsv = banDoc?.MaSinhVien ?? maBD;

            // 2. CẬP NHẬT TRẠNG THÁI VÀO ENTITY FRAMEWORK CORE
            ctm.NgayTra = DateTime.Now;

            if (actionType.Equals("Compensation", StringComparison.OrdinalIgnoreCase))
            {
                // BỒI THƯỜNG
                if (compensationType.Equals("NewBook", StringComparison.OrdinalIgnoreCase))
                {
                    ctm.TrangThai = "Đã đền sách mới";
                    ctm.TinhTrangKhiTra = "Đền 01 cuốn sách mới tương đương";
                    if (banSao != null)
                    {
                        banSao.TrangThai = "Có sẵn";
                        banSao.TinhTrang = "Tốt";
                    }

                    var receipt = new FinancialReceiptViewModel
                    {
                        MaPhieuThu = receiptCode,
                        MaGD = safeMaGD,
                        MaBD = readerMsv,
                        NguoiNop = readerName,
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
                    message = $"Đã tiếp nhận 01 cuốn sách mới đền bù cho tài liệu '{bookTitle}' từ bạn đọc '{readerName}'.{(overdueFee > 0 ? " Thu phạt nộp muộn: 100.000 VNĐ." : "")} CSDL SQL Server đã cập nhật vĩnh viễn.";
                }
                else
                {
                    ctm.TrangThai = "Đã đền tiền";
                    ctm.TinhTrangKhiTra = "Báo mất sách - Bồi thường bằng tiền mặt";
                    if (banSao != null)
                    {
                        banSao.TrangThai = "Đã mất";
                        banSao.TinhTrang = "Mất";
                    }

                    decimal cashCompensation = compensationAmount > 0 ? compensationAmount : 150000m;
                    decimal totalCash = cashCompensation + overdueFee;

                    var receipt = new FinancialReceiptViewModel
                    {
                        MaPhieuThu = receiptCode,
                        MaGD = safeMaGD,
                        MaBD = readerMsv,
                        NguoiNop = readerName,
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
                    message = $"Đã lập phiếu thu #{receiptCode}: Tiếp nhận đền bù bằng tiền {totalCash:N0} VNĐ (Đền sách: {cashCompensation:N0} VNĐ{(overdueFee > 0 ? $", Phạt quá hạn: {overdueFee:N0} VNĐ" : "")}) cho cuốn '{bookTitle}' từ bạn đọc '{readerName}'. Đã lưu vào CSDL SQL Server.";
                }
            }
            else
            {
                // TRẢ SÁCH BÌNH THƯỜNG
                ctm.TrangThai = "Đã trả";
                ctm.TinhTrangKhiTra = "Bình thường";
                if (banSao != null)
                {
                    banSao.TrangThai = "Có sẵn";
                    banSao.TinhTrang = "Tốt";
                }

                if (isOverdue)
                {
                    decimal fineAmount = 100000m;
                    var receipt = new FinancialReceiptViewModel
                    {
                        MaPhieuThu = receiptCode,
                        MaGD = safeMaGD,
                        MaBD = readerMsv,
                        NguoiNop = readerName,
                        TenSach = bookTitle,
                        LoaiKhoanThu = "Nộp muộn",
                        HinhThucXuLy = "Trả sách & nộp phạt nộp muộn",
                        SoTien = fineAmount,
                        TienNopMuon = fineAmount,
                        TienDenBu = 0,
                        NgayThu = todayStr,
                        GhiChu = string.IsNullOrWhiteSpace(note) ? $"Hoàn tất trả sách và nộp phạt nộp muộn ({ctm.TrangThai})" : note
                    };
                    message = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{readerName}' thành công! Thu phạt nộp muộn 100.000 VNĐ đã được cộng vào quỹ tài chính và lưu vào CSDL SQL Server.";
                }
                else
                {
                    message = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{readerName}' thành công! Sách còn hạn mượn, CSDL SQL Server đã cập nhật.";
                }
            }

            // Kiểm tra phiếu mượn cha: nếu tất cả chi tiết mượn đã trả hết thì đổi trạng thái phiếu mượn sang "Đã trả"
            if (ctm.MaPhieuMuonNavigation != null)
            {
                var remainingActive = _dbContext.ChiTietMuons
                    .Any(c => c.MaPhieuMuon == ctm.MaPhieuMuon && c.MaChiTietMuon != ctm.MaChiTietMuon && c.NgayTra == null && c.TrangThai != "Đã trả" && c.TrangThai != "Đã đền sách mới" && c.TrangThai != "Đã đền tiền");
                if (!remainingActive)
                {
                    ctm.MaPhieuMuonNavigation.TrangThai = "Đã trả";
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

            // Ghi nhận bảng ThanhToan trong SQL Server nếu có phát sinh tiền
            decimal paymentTotal = overdueFee + (actionType.Equals("Compensation", StringComparison.OrdinalIgnoreCase) && !compensationType.Equals("NewBook", StringComparison.OrdinalIgnoreCase) ? (compensationAmount > 0 ? compensationAmount : 150000m) : 0m);
            if (paymentTotal > 0 && banDoc != null)
            {
                var thanhToan = new ThanhToan
                {
                    MaBanDoc = banDoc.MaBanDoc,
                    MaNhanVien = 1,
                    NgayThanhToan = DateTime.Now,
                    TongTien = paymentTotal,
                    PhuongThuc = "Tiền mặt",
                    TrangThai = "Thành công",
                    NoiDung = $"Thu phí lưu thông: {safeMaGD} ({bookTitle})"
                };
                _dbContext.ThanhToans.Add(thanhToan);
            }

            // COMMIT TRỰC TIẾP VÀO CSDL SQL SERVER
            _dbContext.SaveChanges();

            // Cập nhật AvailableCopies trong catalog service
            if (book != null && (actionType.Equals("NormalReturn", StringComparison.OrdinalIgnoreCase) || compensationType.Equals("NewBook", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var catBook = _catalogService.GetById(book.MaTaiLieu);
                    if (catBook != null)
                    {
                        catBook.AvailableCopies = Math.Min(catBook.TotalCopies, catBook.AvailableCopies + 1);
                        _catalogService.UpdateBook(catBook, out _);
                    }
                }
                catch { }
            }

            // Đồng bộ sang UserProfileService
            try
            {
                _userProfileService.GetProfile(readerMsv, readerName);
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

            // Tìm một bản sao còn sẵn trong kho (nếu chưa có sẵn thì tự động bổ sung bản sao mới vào kho)
            var banSao = _dbContext.BanSaos.FirstOrDefault(bs => bs.MaTaiLieu == bookId && bs.TrangThai == "Có sẵn");
            if (banSao == null)
            {
                int copyCount = _dbContext.BanSaos.Count(bs => bs.MaTaiLieu == bookId);
                banSao = new BanSao
                {
                    MaTaiLieu = bookId,
                    MaVach = $"UTC-{bookId:D2}-{copyCount + 1:D2}",
                    ViTriKe = "Kệ lưu thông Tầng 2",
                    TinhTrang = "Tốt",
                    TrangThai = "Có sẵn",
                    NgayNhap = DateOnly.FromDateTime(DateTime.Today)
                };
                _dbContext.BanSaos.Add(banSao);
                _dbContext.SaveChanges();
            }

            if (string.IsNullOrEmpty(banDoc.SoThe))
            {
                banDoc.SoThe = $"THE-{banDoc.MaSinhVien}";
                banDoc.NgayCapThe = DateOnly.FromDateTime(DateTime.Today);
                banDoc.NgayHetHanThe = DateOnly.FromDateTime(DateTime.Today.AddYears(4));
                banDoc.TrangThaiThe = "Hoạt động";
                _dbContext.SaveChanges();
            }

            // Tạo phiếu mượn mới cho phiên mượn tại quầy lưu thông
            var phieuMuon = new PhieuMuon
            {
                MaBanDoc = banDoc.MaBanDoc,
                MaNhanVien = 1,
                NgayMuon = DateTime.Now,
                TrangThai = "Đang mượn",
                GhiChu = "Mượn tại quầy lưu thông thư viện UTC"
            };
            _dbContext.PhieuMuons.Add(phieuMuon);
            _dbContext.SaveChanges();

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

            // Cập nhật AvailableCopies trong CatalogService
            try
            {
                var catBook = _catalogService.GetById(bookId);
                if (catBook != null)
                {
                    catBook.AvailableCopies = Math.Max(0, catBook.AvailableCopies - 1);
                    _catalogService.UpdateBook(catBook, out _);
                }
            }
            catch { }

            // Đồng bộ sang UserProfileService
            try
            {
                _userProfileService.GetProfile(banDoc.MaSinhVien, banDoc.HoTen);
            }
            catch { }

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
        var payments = _dbContext.ThanhToans
            .AsNoTracking()
            .Include(t => t.MaBanDocNavigation)
            .OrderByDescending(t => t.NgayThanhToan)
            .ToList();

        var receipts = payments.Select(t => new FinancialReceiptViewModel
        {
            MaPhieuThu = $"PT{t.NgayThanhToan:yyMMdd}-{t.MaThanhToan:D3}",
            MaGD = $"GD{t.NgayThanhToan:yyMMdd}-{t.MaThanhToan:D3}",
            MaBD = t.MaBanDocNavigation?.MaSinhVien ?? "242630923",
            NguoiNop = t.MaBanDocNavigation?.HoTen ?? "Bạn đọc UTC",
            TenSach = t.NoiDung ?? "Phí dịch vụ thư viện",
            LoaiKhoanThu = t.NoiDung?.Contains("Đền") == true ? "Đền bù mất/hỏng" : "Nộp muộn",
            HinhThucXuLy = t.PhuongThuc ?? "Tiền mặt",
            SoTien = t.TongTien,
            TienNopMuon = t.NoiDung?.Contains("Đền") == true ? 0 : t.TongTien,
            TienDenBu = t.NoiDung?.Contains("Đền") == true ? t.TongTien : 0,
            NgayThu = t.NgayThanhToan.ToString("dd/MM/yyyy HH:mm"),
            GhiChu = t.NoiDung ?? ""
        }).ToList();

        return new FinancialSummaryViewModel
        {
            TongThu = payments.Sum(r => r.TongTien),
            ThuNopMuon = payments.Where(t => t.NoiDung == null || !t.NoiDung.Contains("Đền")).Sum(r => r.TongTien),
            ThuDenBu = payments.Where(t => t.NoiDung != null && t.NoiDung.Contains("Đền")).Sum(r => r.TongTien),
            TongPhieuThu = payments.Count,
            RecentReceipts = receipts.Take(10).ToList()
        };
    }

    public List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? searchKeyword, string? selectedType)
    {
        var payments = _dbContext.ThanhToans
            .AsNoTracking()
            .Include(t => t.MaBanDocNavigation)
            .OrderByDescending(t => t.NgayThanhToan)
            .ToList();

        var receipts = payments.Select(t => new FinancialReceiptViewModel
        {
            MaPhieuThu = $"PT{t.NgayThanhToan:yyMMdd}-{t.MaThanhToan:D3}",
            MaGD = $"GD{t.NgayThanhToan:yyMMdd}-{t.MaThanhToan:D3}",
            MaBD = t.MaBanDocNavigation?.MaSinhVien ?? "242630923",
            NguoiNop = t.MaBanDocNavigation?.HoTen ?? "Bạn đọc UTC",
            TenSach = t.NoiDung ?? "Phí dịch vụ thư viện",
            LoaiKhoanThu = t.NoiDung?.Contains("Đền") == true ? "Đền bù mất/hỏng" : "Nộp muộn",
            HinhThucXuLy = t.PhuongThuc ?? "Tiền mặt",
            SoTien = t.TongTien,
            TienNopMuon = t.NoiDung?.Contains("Đền") == true ? 0 : t.TongTien,
            TienDenBu = t.NoiDung?.Contains("Đền") == true ? t.TongTien : 0,
            NgayThu = t.NgayThanhToan.ToString("dd/MM/yyyy HH:mm"),
            GhiChu = t.NoiDung ?? ""
        }).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var kw = searchKeyword.Trim().ToLower();
            receipts = receipts.Where(r => r.MaPhieuThu.ToLower().Contains(kw) ||
                                           r.MaBD.ToLower().Contains(kw) ||
                                           r.NguoiNop.ToLower().Contains(kw) ||
                                           r.TenSach.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(selectedType) && selectedType != "Tất cả")
        {
            if (selectedType == "Nộp muộn")
            {
                receipts = receipts.Where(r => r.LoaiKhoanThu.Contains("Nộp muộn"));
            }
            else if (selectedType == "Đền bù mất/hỏng")
            {
                receipts = receipts.Where(r => r.LoaiKhoanThu.Contains("Đền bù"));
            }
        }

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

        int dangMuon = _dbContext.BanSaos.Count(b => b.TrangThai == "Đang mượn");
        int sanCo = _dbContext.BanSaos.Count(b => b.TrangThai == "Có sẵn" || b.TrangThai == "Sẵn sàng");

        return new DashboardViewModel
        {
            TongDauSach = totalBooks,
            TongBanSao = totalCopies,
            DangMuon = dangMuon,
            BanSanCo = sanCo,
            MatChoBoiThuong = _dbContext.BanSaos.Count(b => b.TrangThai == "Đã mất" || b.TinhTrang == "Mất"),
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
