using System.ComponentModel.DataAnnotations;

namespace BTL_LTWEB.Areas.Admin.ViewModels;

public class ReaderListItemViewModel
{
    public string MaBD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Nhom { get; set; } = string.Empty;
    public string LopKhoa { get; set; } = string.Empty;
    public string QuyenSuDung { get; set; } = string.Empty;
    public int SoSachDangMuon { get; set; }
    public int SoSachQuaHan { get; set; }
}

public class ReaderIndexViewModel
{
    public string? SearchKeyword { get; set; }
    public string? SelectedGroup { get; set; }
    public List<string> Groups { get; set; } = new() { "Tất cả nhóm", "Sinh viên", "Giảng viên", "Ngoài trường" };
    public List<ReaderListItemViewModel> Readers { get; set; } = new();
}

public class ReaderDetailViewModel
{
    public string MaBD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string LopKhoa { get; set; } = string.Empty;
    public string TrangThaiHocTap { get; set; } = string.Empty;

    [Display(Name = "Nhóm bạn đọc")]
    public string Nhom { get; set; } = string.Empty;

    [Display(Name = "Quyền sử dụng thư viện")]
    public string QuyenSuDung { get; set; } = string.Empty;

    [Display(Name = "Giới hạn số sách")]
    public string GioiHanSach { get; set; } = "5 cuốn";

    [Display(Name = "Hiệu lực quyền sử dụng")]
    public string HieuLucQuyen { get; set; } = "2027-06-30";

    public int SoSachDangMuon { get; set; } = 2;
    public int SoSachQuaHan { get; set; } = 1;

    public List<BorrowTransactionViewModel> BorrowTransactions { get; set; } = new();
}

public class BorrowTransactionViewModel
{
    public string MaGD { get; set; } = string.Empty;
    public string MaSach { get; set; } = string.Empty;
    public string TenSach { get; set; } = string.Empty;
    public string NgayMuon { get; set; } = string.Empty;
    public string NgayHenTra { get; set; } = string.Empty;
    public string? NgayTra { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool IsQuaHan { get; set; }
}

public class ImportQldtItemViewModel
{
    public string MaSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string LopKhoa { get; set; } = string.Empty;
    public string TrangThaiKiemTra { get; set; } = string.Empty;
    public bool IsNew { get; set; }
    public bool IsValid { get; set; } = true;
}

public class ImportQldtViewModel
{
    public string NguonDuLieu { get; set; } = "Danh sách sinh viên từ QLĐT";
    public string TepDuLieu { get; set; } = "sinh-vien-k65.xlsx (minh họa)";
    public List<ImportQldtItemViewModel> PreviewItems { get; set; } = new();
    public int TotalValid => PreviewItems.Count(x => x.IsValid);
    public int TotalError => PreviewItems.Count(x => !x.IsValid);
}

