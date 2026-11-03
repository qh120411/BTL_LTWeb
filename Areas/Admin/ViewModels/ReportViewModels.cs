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
    public string KyBaoCao { get; set; } = "Tháng 10/2026";
    public string PhamVi { get; set; } = "Toàn thư viện";

    public List<string> DanhSachKyBaoCao { get; set; } = new()
    {
        "Tháng 10/2026",
        "Tháng 09/2026",
        "Tháng 08/2026",
        "Quý 3/2026",
        "Năm học 2025 - 2026"
    };

    public List<string> DanhSachPhamVi { get; set; } = new()
    {
        "Toàn thư viện",
        "Cơ sở chính (Số 3 Cầu Giấy)",
        "Phân hiệu TP. Hồ Chí Minh",
        "Phòng mượn tự chọn",
        "Phòng đọc chuyên ngành"
    };

    // 4 Card Thống Kê
    public int LuotMuonTrongThang { get; set; } = 42;
    public int LuotTraTrongThang { get; set; } = 37;
    public int QuaHanHienTai { get; set; } = 1;
    public int TongBanSach { get; set; } = 65;

    // Chi tiết từng đầu sách
    public List<BookReportItemViewModel> ChiTietDauSach { get; set; } = new();

    // Dòng TỔNG
    public int TongCongSoBan => ChiTietDauSach.Sum(x => x.TongBan);
    public int TongCongSanCo => ChiTietDauSach.Sum(x => x.SanCo);
    public int TongCongDangMuon => ChiTietDauSach.Sum(x => x.DangMuon);
    public int TongCongMatHong => ChiTietDauSach.Sum(x => x.MatHong);
}

