namespace BTL_LTWEB.Areas.Admin.ViewModels;

public class BookReportItemViewModel
{
    public string DauSach { get; set; } = string.Empty;
    public int TongBan { get; set; }
    public int SanCo { get; set; }
    public int DangMuon { get; set; }
    public int MatHong { get; set; }
}

public class ReportViewModel
{
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string KyBaoCao { get; set; } = string.Empty;
    public string PhamVi { get; set; } = "Thư viện Đại học Giao thông Vận tải (Nhà A8)";

    // 4 Card Thống Kê
    public int LuotMuonTrongThang { get; set; } = 42;
    public int LuotTraTrongThang { get; set; } = 37;
    public int QuaHanHienTai { get; set; } = 1;
    public int TongBanSach { get; set; } = 65;

    // Chi tiết từng đầu sách
    public List<BookReportItemViewModel> ChiTietDauSach { get; set; } = new();

    // Dữ liệu cho các biểu đồ (Charts)
    public List<string> MonthlyLabels { get; set; } = new();
    public List<int> MonthlyBorrowData { get; set; } = new();
    public List<int> MonthlyReturnData { get; set; } = new();

    // Tỷ lệ phân bố tình trạng sách
    public int TyLeSanCo { get; set; }
    public int TyLeDangMuon { get; set; }
    public int TyLeMatHong { get; set; }

    // Top 5 sách mượn nhiều nhất
    public List<string> TopBookLabels { get; set; } = new();
    public List<int> TopBookBorrowCounts { get; set; } = new();

    // Phân bố bạn đọc mượn sách theo nhóm
    public int SoLuongSinhVienMuon { get; set; }
    public int SoLuongGiangVienMuon { get; set; }

    // Dòng TỔNG
    public int TongCongSoBan => ChiTietDauSach.Sum(x => x.TongBan);
    public int TongCongSanCo => ChiTietDauSach.Sum(x => x.SanCo);
    public int TongCongDangMuon => ChiTietDauSach.Sum(x => x.DangMuon);
    public int TongCongMatHong => ChiTietDauSach.Sum(x => x.MatHong);
}

