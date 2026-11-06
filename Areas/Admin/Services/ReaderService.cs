using System.Data;
using System.Security.Claims;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Services;
// Tất cả dữ liệu nghiệp vụ đọc/ghi cùng DbContext; không dùng JSON dự phòng.
public class ReaderService(QLThuVienUtcContext db, IBookCatalogService catalog, IHttpContextAccessor http, ILogger<ReaderService> logger) : IReaderService
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private int StaffId => int.TryParse(http.HttpContext?.User.FindFirstValue("StaffId"), out var id) ? id : throw new InvalidOperationException("Tài khoản chưa liên kết nhân viên.");

    private IQueryable<ChiTietMuon> Loans() => db.ChiTietMuons.Include(c => c.MaPhieuMuonNavigation).ThenInclude(p => p.MaTheBanDocNavigation).ThenInclude(t => t.MaBanDocNavigation).Include(c => c.MaBanSaoNavigation).ThenInclude(b => b.MaTaiLieuNavigation);
    public ReaderIndexViewModel GetReaders(string? keyword, string? group)
    {
        var loans = Loans().AsNoTracking().Where(c => c.NgayTra == null).ToList();
        var rows = db.BanDocs.AsNoTracking().Include(b => b.MaLopNavigation).Include(b => b.TheBanDoc).ToList();
        var k = BookCatalogService.Normalize(keyword);
        return new ReaderIndexViewModel
        {
            SearchKeyword = keyword,
            SelectedGroup = group,
            Readers = rows.Where(b => k.Length == 0 || BookCatalogService.Normalize(b.MaSinhVien + " " + b.HoTen).Contains(k)).Select(b => new ReaderListItemViewModel { MaBD = b.MaSinhVien, HoTen = b.HoTen, LopKhoa = b.MaLopNavigation?.TenLop ?? "", Nhom = "Sinh viên", QuyenSuDung = b.TheBanDoc?.TrangThai ?? "Chưa cấp thẻ", SoSachDangMuon = loans.Count(c => c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDoc == b.MaBanDoc), SoSachQuaHan = loans.Count(c => c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDoc == b.MaBanDoc && c.HanTra < Today) }).Where(b => string.IsNullOrEmpty(group) || group == "Tất cả nhóm" || group == b.Nhom).ToList()
        };
    }

    public ReaderDetailViewModel? GetReaderById(string id)
    {
        var b = db.BanDocs.Include(b => b.TheBanDoc).Include(b => b.MaLopNavigation).FirstOrDefault(b => b.MaSinhVien == id);
        if (b == null)
            return null;
        return new ReaderDetailViewModel
        {
            MaBD = id,
            HoTen = b.HoTen,
            Nhom = "Sinh viên",
            LopKhoa = b.MaLopNavigation?.TenLop ?? "",
            TrangThaiHocTap = b.TrangThai,
            QuyenSuDung = b.TheBanDoc?.TrangThai ?? "Chưa cấp thẻ",
            HieuLucQuyen = b.TheBanDoc?.NgayHetHan?.ToString("yyyy-MM-dd") ?? "",
            AvailableBooks = catalog.GetAll().ToList(),
            BorrowTransactions = Loans().AsNoTracking().Where(c => c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDoc == b.MaBanDoc && c.NgayTra == null).ToList().Select(c => new BorrowTransactionViewModel { MaGD = c.MaChiTietMuon.ToString(), MaSach = c.MaBanSaoNavigation.MaVach, TenSach = c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu, NgayMuon = c.MaPhieuMuonNavigation.NgayMuon.ToString("dd/MM/yyyy"), NgayHenTra = c.HanTra.ToString("dd/MM/yyyy"), IsQuaHan = c.HanTra < Today, TrangThai = c.TrangThai }).ToList()
        };
    }

    public bool UpdateReaderSettings(ReaderDetailViewModel m)
    {
        var b = db.BanDocs.Include(b => b.TheBanDoc).FirstOrDefault(b => b.MaSinhVien == m.MaBD);
        if (b?.TheBanDoc == null || !DateOnly.TryParse(m.HieuLucQuyen, out var expiry) || !(new[]
        {
            "Hoạt động",
            "Bị khóa",
            "Tạm khóa"
        }

        ).Contains(m.QuyenSuDung))
            return false;
        b.TheBanDoc.TrangThai = m.QuyenSuDung;
        b.TheBanDoc.NgayHetHan = expiry;
        db.SaveChanges();
        return true;
    }

    public bool BorrowBook(string readerId, int bookId, out string title, out string error)
    {
        title = "";
        error = "";
        using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var b = db.BanDocs.Include(b => b.TheBanDoc).FirstOrDefault(b => b.MaSinhVien == readerId);
            if (b?.TheBanDoc == null || b.TheBanDoc.TrangThai != "Hoạt động" || b.TheBanDoc.NgayHetHan < Today || b.TrangThai != "Đang học")
            {
                error = "Bạn đọc chưa có thẻ hợp lệ hoặc đã bị khóa/hết hạn.";
                return false;
            }

            var loans = Loans().Where(c => c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDoc == b.MaBanDoc && c.NgayTra == null).ToList();
            if (loans.Count >= 5)
            {
                error = "Đã đạt giới hạn 5 cuốn.";
                return false;
            }

            if (loans.Any(c => c.HanTra < Today))
            {
                error = "Bạn đọc có sách quá hạn, cần xử lý trước khi mượn thêm.";
                return false;
            }

            if (db.TienPhats.Any(p => p.MaViPhamNavigation.MaBanDoc == b.MaBanDoc && p.TrangThai != "Đã thanh toán"))
            {
                error = "Bạn đọc còn khoản phạt chưa xử lý.";
                return false;
            }

            if (loans.Any(c => c.MaBanSaoNavigation.MaTaiLieu == bookId))
            {
                error = "Bạn đọc đang mượn đầu sách này.";
                return false;
            }

            var copy = db.BanSaos.Include(c => c.MaTaiLieuNavigation).FirstOrDefault(c => c.MaTaiLieu == bookId && c.TrangThai == "Có sẵn");
            if (copy == null)
            {
                error = "Không còn bản sách sẵn có.";
                return false;
            }

            var claimed = db.BanSaos.Where(c => c.MaBanSao == copy.MaBanSao && c.TrangThai == "Có sẵn").ExecuteUpdate(s => s.SetProperty(c => c.TrangThai, "Đang mượn"));
            if (claimed != 1)
            {
                error = "Bản sách vừa được người khác mượn. Hãy thử lại.";
                return false;
            }

            var loan = new PhieuMuon
            {
                MaTheBanDoc = b.TheBanDoc.MaTheBanDoc,
                MaNhanVien = StaffId,
                NgayMuon = DateTime.Now,
                TrangThai = "Đang mượn"
            };
            loan.ChiTietMuons.Add(new ChiTietMuon { MaBanSao = copy.MaBanSao, HanTra = Today.AddYears(1), TrangThai = "Đang mượn" });
            db.PhieuMuons.Add(loan);
            db.SaveChanges();
            tx.Commit();
            title = copy.MaTaiLieuNavigation.TenTaiLieu;
            return true;
        }
        catch (Exception ex)when (ex is DbUpdateException or System.Data.Common.DbException)
        {
            logger.LogError(ex, "Lỗi mượn sách");
            error = "Không lưu được phiếu mượn. Thử lại sau.";
            return false;
        }
    }

    public bool ReturnBook(string reader, string transaction, out string title, out decimal fine)
    {
        title = "";
        fine = 0;
        var c = Loans().AsNoTracking().FirstOrDefault(c => c.MaChiTietMuon.ToString() == transaction && c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.MaSinhVien == reader);
        if (c == null || c.HanTra < Today)
            return false; // quá hạn phải xác nhận thu phạt bằng form xử lý.
        title = c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu;
        return ProcessReturnOrCompensation(reader, transaction, "NormalReturn", "NewBook", 0, false, null, out _);
    }

    public bool ProcessReturnOrCompensation(string reader, string transaction, string action, string compensation, decimal amount, bool payOverdue, string? note, out string message)
    {
        message = "";
        if (action != "NormalReturn" && action != "Compensation")
        {
            message = "Thao tác không hợp lệ.";
            return false;
        }

        if (action == "Compensation" && (compensation != "NewBook" && compensation != "Money"))
        {
            message = "Hình thức bồi thường không hợp lệ.";
            return false;
        }

        if (action == "Compensation" && compensation == "Money" && (amount <= 0 || amount > 100000000))
        {
            message = "Nhập số tiền bồi thường hợp lệ.";
            return false;
        }

        using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var c = Loans().Include(c => c.ViPhams).ThenInclude(v => v.TienPhat).FirstOrDefault(c => c.MaChiTietMuon.ToString() == transaction && c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.MaSinhVien == reader);
            if (c == null || c.NgayTra != null)
            {
                message = "Sai mã giao dịch hoặc giao dịch đã hoàn tất.";
                return false;
            }

            var overdue = c.HanTra < Today;
            if (overdue && !payOverdue)
            {
                message = "Cần xác nhận thu phạt quá hạn trước khi đóng giao dịch.";
                return false;
            }

            c.NgayTra = DateTime.Now;
            c.TrangThai = action == "NormalReturn" ? "Đã trả" : compensation == "NewBook" ? "Đã đền sách mới" : "Đã đền tiền";
            c.TinhTrangKhiTra = action == "NormalReturn" ? "Bình thường" : compensation == "NewBook" ? "Đã nhận sách thay thế" : "Mất";
            c.MaBanSaoNavigation.TrangThai = action == "Compensation" && compensation == "Money" ? "Đã mất" : "Có sẵn";
            var readerId = c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDoc;
            var payments = new List<TienPhat>();
            foreach (var fee in c.ViPhams.Select(v => v.TienPhat).Where(p => p != null && p.TrangThai != "Đã thanh toán"))
                payments.Add(fee!);
            void Fee(string type, decimal value)
            {
                var v = new ViPham
                {
                    MaBanDoc = readerId,
                    MaChiTietMuon = c.MaChiTietMuon,
                    LoaiViPham = type,
                    NgayViPham = DateTime.Now,
                    MoTa = note
                };
                var f = new TienPhat
                {
                    MaViPhamNavigation = v,
                    SoTien = value,
                    TrangThai = "Đã thanh toán"
                };
                db.TienPhats.Add(f);
                payments.Add(f);
            }

            if (overdue && !c.ViPhams.Any(v => v.LoaiViPham == "Quá hạn"))
                Fee("Quá hạn", 100000);
            if (action == "Compensation" && compensation == "Money")
                Fee("Mất / hỏng", amount);
            if (payments.Count > 0)
            {
                var receipt = new ThanhToan
                {
                    MaBanDoc = readerId,
                    MaNhanVien = StaffId,
                    NgayThanhToan = DateTime.Now,
                    PhuongThuc = "Tiền mặt",
                    TrangThai = "Thành công",
                    TongTien = payments.Sum(f => f.SoTien)
                };
                foreach (var fee in payments)
                {
                    fee.TrangThai = "Đã thanh toán";
                    receipt.ChiTietThanhToans.Add(new ChiTietThanhToan { MaTienPhatNavigation = fee, SoTien = fee.SoTien });
                }

                db.ThanhToans.Add(receipt);
            }

            if (!db.ChiTietMuons.Any(x => x.MaPhieuMuon == c.MaPhieuMuon && x.MaChiTietMuon != c.MaChiTietMuon && x.NgayTra == null))
                c.MaPhieuMuonNavigation.TrangThai = "Đã trả";
            db.SaveChanges();
            tx.Commit();
            message = "Đã cập nhật giao dịch và tồn kho.";
            return true;
        }
        catch (Exception ex)when (ex is DbUpdateException or System.Data.Common.DbException)
        {
            logger.LogError(ex, "Lỗi trả sách");
            message = "Không lưu được giao dịch. Dữ liệu chưa được thay đổi.";
            return false;
        }
    }

    public List<CirculationTransactionItem> GetAllCirculationTransactions() => Loans().AsNoTracking().Where(c => c.NgayTra == null).ToList().Select(c => new CirculationTransactionItem { MaGD = c.MaChiTietMuon.ToString(), MaBD = c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.MaSinhVien, HoTen = c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.HoTen, MaBanSao = c.MaBanSaoNavigation.MaVach, TenSach = c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu, NgayMuon = c.MaPhieuMuonNavigation.NgayMuon.ToString("dd/MM/yyyy"), HanTra = c.HanTra.ToString("dd/MM/yyyy"), TrangThai = c.HanTra < Today ? "Quá hạn" : c.TrangThai, IsQuaHan = c.HanTra < Today }).ToList();
    public Dictionary<string, List<string>> GetReaderBorrowedTitlesMap() => GetAllCirculationTransactions().GroupBy(c => c.MaBD).ToDictionary(g => g.Key, g => g.Select(c => c.TenSach).ToList());
    public List<OverdueItemViewModel> GetAllOverdueLoans() => Loans().AsNoTracking().Where(c => c.NgayTra == null && c.HanTra < Today).ToList().Select(c => new OverdueItemViewModel { MaGD = c.MaChiTietMuon.ToString(), MaBD = c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.MaSinhVien, HoTen = c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.HoTen, TenSach = c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu, HanTra = c.HanTra.ToString("dd/MM/yyyy"), NgayMuon = c.MaPhieuMuonNavigation.NgayMuon.ToString("dd/MM/yyyy"), SoNgayQuaHan = Today.DayNumber - c.HanTra.DayNumber }).ToList();
    public List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? keyword, string? type)
    {
        var rows = db.ThanhToans.AsNoTracking().Include(t => t.MaBanDocNavigation).Include(t => t.ChiTietThanhToans).ThenInclude(c => c.MaTienPhatNavigation).ThenInclude(p => p.MaViPhamNavigation).ToList();
        var k = BookCatalogService.Normalize(keyword);
        return rows.OrderByDescending(t => t.NgayThanhToan).Select(t => new FinancialReceiptViewModel { MaPhieuThu = "PT" + t.MaThanhToan, MaBD = t.MaBanDocNavigation.MaSinhVien, NguoiNop = t.MaBanDocNavigation.HoTen, SoTien = t.TongTien, NgayThu = t.NgayThanhToan.ToString("dd/MM/yyyy HH:mm"), HinhThucXuLy = t.PhuongThuc, LoaiKhoanThu = string.Join(", ", t.ChiTietThanhToans.Select(c => c.MaTienPhatNavigation.MaViPhamNavigation.LoaiViPham).Distinct()), TienNopMuon = t.ChiTietThanhToans.Where(c => c.MaTienPhatNavigation.MaViPhamNavigation.LoaiViPham == "Quá hạn").Sum(c => c.SoTien), TienDenBu = t.ChiTietThanhToans.Where(c => c.MaTienPhatNavigation.MaViPhamNavigation.LoaiViPham != "Quá hạn").Sum(c => c.SoTien) }).Where(t => k.Length == 0 || BookCatalogService.Normalize(t.MaBD + " " + t.NguoiNop).Contains(k)).Where(t => string.IsNullOrEmpty(type) || type == "Tất cả" || t.LoaiKhoanThu.Contains(type)).ToList();
    }

    public FinancialSummaryViewModel GetFinancialSummary()
    {
        var rows = GetAllFinancialReceipts(null, null);
        return new()
        {
            TongThu = rows.Sum(t => t.SoTien),
            ThuNopMuon = rows.Sum(t => t.TienNopMuon),
            ThuDenBu = rows.Sum(t => t.TienDenBu),
            TongPhieuThu = rows.Count,
            RecentReceipts = rows.Take(8).ToList()
        };
    }

    public DashboardViewModel GetDashboardStatistics() => new()
    {
        TongDauSach = db.TaiLieus.Count(),
        TongBanSao = db.BanSaos.Count(),
        BanSanCo = db.BanSaos.Count(b => b.TrangThai == "Có sẵn"),
        DangMuon = db.ChiTietMuons.Count(c => c.NgayTra == null),
        MatChoBoiThuong = db.BanSaos.Count(b => b.TrangThai == "Đã mất" || b.TrangThai == "Mất"),
        LuotQuaHan = db.ChiTietMuons.Count(c => c.NgayTra == null && c.HanTra < Today),
        TongBanDoc = db.BanDocs.Count(),
        TongQuyTaiChinh = GetFinancialSummary().TongThu,
        RecentTransactions = GetAllCirculationTransactions().Take(8).Select(c => new DashboardTransactionItem { MaGD = c.MaGD, MaBD = c.MaBD, HoTen = c.HoTen, TenSach = c.TenSach, HanTra = c.HanTra, NgayMuon = c.NgayMuon, TrangThai = c.TrangThai, IsQuaHan = c.IsQuaHan }).ToList()
    };
    public ImportQldtViewModel GetImportPreview() => new()
    {
        NguonDuLieu = "Chưa kết nối QLĐT",
        TepDuLieu = ""
    };
    public bool ProcessImport(out int updated, out int inserted)
    {
        updated = inserted = 0;
        return false;
    }
}
