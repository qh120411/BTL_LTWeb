using BTL_LTWEB.Areas.Reader.Models;

namespace BTL_LTWEB.Areas.Admin.ViewModels
{
    public class DashboardTransactionItem
    {
        public string MaGD { get; set; } = string.Empty;
        public string MaBD { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string MaSach { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public string NgayMuon { get; set; } = string.Empty;
        public string HanTra { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public bool IsQuaHan { get; set; }
        public int SoNgayQuaHan { get; set; }
        public decimal TienPhat { get; set; }
    }

    public class DashboardViewModel
    {
        // 4 Card Thống kê chuẩn theo CSDL thật
        public int TongDauSach { get; set; } = 30; // 30 đầu sách trong CSDL
        public int TongBanSao { get; set; } = 120; // 120 bản sao vật lý
        public int BanSanCo { get; set; } = 102;   // 102 bản sẵn có
        public int DangMuon { get; set; } = 18;    // 18 bản đang lưu hành ngoài kho
        public int MatChoBoiThuong { get; set; } = 1; // 1 sách mất chờ bồi thường
        public int LuotQuaHan { get; set; } = 8;   // 8 cuốn quá hạn (Đỗ Văn Đạt 3 cuốn,...)
        public int TongBanDoc { get; set; } = 50;  // 49 sinh viên K65.CLCCNTT1 + 1 giảng viên

        // Danh sách giao dịch mượn trả gần đây nhất
        public List<DashboardTransactionItem> RecentTransactions { get; set; } = new();

        // Thống kê tài chính & Quỹ thư viện UTC
        public decimal TongQuyTaiChinh { get; set; }
        public decimal TienThuNopMuon { get; set; }
        public decimal TienThuDenBu { get; set; }
        public List<FinancialReceiptViewModel> RecentReceipts { get; set; } = new();
    }

    public class CatalogItemViewModel
    {
        public int Id { get; set; }
        public string MaHocLieu { get; set; } = string.Empty;
        public string TenHocLieu { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public string NhaXuatBan { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int NamXuatBan { get; set; } = 2023;
        public int SoTrang { get; set; } = 300;
        public string TheLoai { get; set; } = string.Empty;
        public string MajorCode { get; set; } = "cntt";
        public string DinhDang { get; set; } = "Sách in";
        public bool IsOnline { get; set; }
        public string CoverImage { get; set; } = string.Empty;
        public int TongBan { get; set; } = 4;
        public int SanCo { get; set; } = 4;
        public int DangMuon { get; set; } = 0;
        public string ViTriKe { get; set; } = "Kệ IT-01";
        public string MoTa { get; set; } = string.Empty;
        public string TrangThaiText => SanCo > 0 ? $"Có sẵn ({SanCo}/{TongBan})" : $"Hết sách (0/{TongBan})";
        public List<BookCopyItem> Copies { get; set; } = new();
    }

    public class CatalogIndexViewModel
    {
        public string? SearchKeyword { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedMajor { get; set; }
        public string? SelectedStatus { get; set; }
        public string SelectedLanguage { get; set; } = "all";
        public int TrangHienTai { get; set; } = 1;
        public int KichThuocTrang => 12;
        public int TongTrang => Math.Max(1, (int)Math.Ceiling(Books.Count / (double)KichThuocTrang));
        public IEnumerable<CatalogItemViewModel> BooksOnPage => Books.Skip((TrangHienTai - 1) * KichThuocTrang).Take(KichThuocTrang);
        public IReadOnlyList<int?> TrangHienThi => BTL_LTWEB.Common.PageNavigation.GetPages(TrangHienTai, TongTrang);
        public List<CatalogItemViewModel> Books { get; set; } = new();

        public int TongDauSach => Books.Count;
        public int TongBanSao => Books.Sum(b => b.TongBan);
        public int TongSanCo => Books.Sum(b => b.SanCo);
        public int TongDangMuon => Books.Sum(b => b.DangMuon);
    }

    public class CirculationTransactionItem
    {
        public string MaGD { get; set; } = string.Empty;
        public string MaBD { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string MaBanSao { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public string NgayMuon { get; set; } = string.Empty;
        public string HanTra { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public bool IsQuaHan { get; set; }
    }

    public class CirculationIndexViewModel
    {
        public string? SearchKeyword { get; set; }
        public string? SelectedStatus { get; set; }
        public List<CirculationTransactionItem> Transactions { get; set; } = new();
        public List<BookItem> AvailableBooks { get; set; } = new();
    }

    public class OverdueItemViewModel
    {
        public string MaGD { get; set; } = string.Empty;
        public string MaBD { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public string NgayMuon { get; set; } = string.Empty;
        public string HanTra { get; set; } = string.Empty;
        public int SoNgayQuaHan { get; set; }
        public decimal TienPhat { get; set; } = 100000;
        public string TrangThaiText => $"Quá hạn {SoNgayQuaHan} ngày";
    }

    public class OverdueIndexViewModel
    {
        public string? SearchKeyword { get; set; }
        public string? SelectedLevel { get; set; }
        public List<OverdueItemViewModel> OverdueList { get; set; } = new();
        public decimal TongTienPhat => OverdueList.Sum(x => x.TienPhat);
    }

    public class FinancialReceiptViewModel
    {
        public string MaPhieuThu { get; set; } = string.Empty;
        public string MaGD { get; set; } = string.Empty;
        public string MaBD { get; set; } = string.Empty;
        public string NguoiNop { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public string LoaiKhoanThu { get; set; } = string.Empty; // "Nộp muộn" hoặc "Đền bù mất/hỏng"
        public string HinhThucXuLy { get; set; } = string.Empty; // "Trả sách & nộp phạt", "Đền tiền mất sách", "Đền sách mới"
        public decimal SoTien { get; set; }
        public decimal TienNopMuon { get; set; }
        public decimal TienDenBu { get; set; }
        public string NgayThu { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
    }

    public class FinancialSummaryViewModel
    {
        public decimal TongThu { get; set; }
        public decimal ThuNopMuon { get; set; }
        public decimal ThuDenBu { get; set; }
        public int TongPhieuThu { get; set; }
        public List<FinancialReceiptViewModel> RecentReceipts { get; set; } = new();
    }

    public class IncidentIndexViewModel
    {
        public decimal TongThu { get; set; }
        public decimal ThuNopMuon { get; set; }
        public decimal ThuDenBu { get; set; }
        public string? SearchKeyword { get; set; }
        public string? SelectedType { get; set; }
        public List<FinancialReceiptViewModel> Receipts { get; set; } = new();
    }
}
