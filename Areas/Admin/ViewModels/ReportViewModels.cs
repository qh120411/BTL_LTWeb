namespace BTL_LTWEB.Areas.Admin.ViewModels;

public class BookReportItemViewModel
{
    public int MaTaiLieu { get; set; }
    public string DauSach { get; set; } = string.Empty;
    public int TongBan { get; set; }
    public int SanCo { get; set; }
    public int DangMuon { get; set; }
    public int MatHong { get; set; }
    public int Khac => TongBan - SanCo - DangMuon - MatHong;
}

public class ReportViewModel
{
    public string KyBaoCao { get; set; } = string.Empty;
    public List<string> DanhSachKyBaoCao { get; set; } = new();
    public string? LoiDuLieu { get; set; }
    public int LuotMuonTrongThang { get; set; }
    public int LuotTraTrongThang { get; set; }
    public int QuaHanHienTai { get; set; }
    public int TongBanDoc { get; set; }
    public int TongDauSach { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? LoiBoLoc { get; set; }
    public string TimKiem { get; set; } = string.Empty;
    public int TrangHienTai { get; set; } = 1;
    public int KichThuocTrang { get; set; } = 10;
    public int TongKetQua { get; set; }
    public int TongTrang => Math.Max(1, (int)Math.Ceiling(TongKetQua / (double)KichThuocTrang));
    public IReadOnlyList<int?> TrangHienThi => BTL_LTWEB.Common.PageNavigation.GetPages(TrangHienTai, TongTrang);
    public InventoryTotalsViewModel TongKho { get; set; } = new();
    public InventoryTotalsViewModel KhoDaLoc { get; set; } = new();
    public List<CirculationTrendItem> XuHuong { get; set; } = new();
    public int TongBanSach => TongCongSoBan;
    public List<BookReportItemViewModel> ChiTietDauSach { get; set; } = new();
    public int TongCongSoBan => TongKho.TongBan;
    public int TongCongSanCo => TongKho.SanCo;
    public int TongCongDangMuon => TongKho.DangMuon;
    public int TongCongMatHong => TongKho.MatHong;
    public int TongCongKhac => TongKho.Khac;
    public bool CoBanKhac => KhoDaLoc.Khac > 0;
}

public class InventoryTotalsViewModel
{
    public int TongBan { get; set; }
    public int SanCo { get; set; }
    public int DangMuon { get; set; }
    public int MatHong { get; set; }
    public int Khac => TongBan - SanCo - DangMuon - MatHong;
}

public class CirculationTrendItem
{
    public string Thang { get; set; } = string.Empty;
    public int Muon { get; set; }
    public int Tra { get; set; }
}
