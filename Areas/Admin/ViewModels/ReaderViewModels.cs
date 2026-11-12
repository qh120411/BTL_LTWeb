using System.ComponentModel.DataAnnotations;
using BTL_LTWEB.Areas.Reader.Models;

namespace BTL_LTWEB.Areas.Admin.ViewModels;

public class BorrowReaderSearchItem
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int ReaderId { get; set; }
    public string MaBD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string LopKhoa { get; set; } = string.Empty;
    public int SoSachDangMuon { get; set; }
    public int GioiHanSach { get; set; }
    public List<string> BorrowedTitles { get; set; } = new();
}

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
    public AdminPaginationViewModel BorrowingPagination { get; set; } = new();
    public AdminPaginationViewModel NonBorrowingPagination { get; set; } = new();
    public IEnumerable<ReaderListItemViewModel> BorrowingReadersOnPage => BorrowingReaders.Skip(BorrowingPagination.Skip).Take(BorrowingPagination.PageSize);
    public IEnumerable<ReaderListItemViewModel> NonBorrowingReadersOnPage => NonBorrowingReaders.Skip(NonBorrowingPagination.Skip).Take(NonBorrowingPagination.PageSize);

    // 2 Bảng tách biệt: Đang mượn sách vs Không mượn sách
    public List<ReaderListItemViewModel> BorrowingReaders => Readers.Where(r => r.SoSachDangMuon > 0).ToList();
    public List<ReaderListItemViewModel> NonBorrowingReaders => Readers.Where(r => r.SoSachDangMuon == 0).ToList();
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
    public string HieuLucQuyen { get; set; } = string.Empty;

    public int SoSachDangMuon => BorrowTransactions.Count;
    public int SoSachQuaHan => BorrowTransactions.Count(b => b.IsQuaHan);
    public decimal TongTienPhat => BorrowTransactions.Where(b => b.IsQuaHan).Sum(b => b.TienPhat);

    // Danh sách sách đang mượn (sổ ra trực tiếp)
    public List<BorrowTransactionViewModel> BorrowTransactions { get; set; } = new();

    // Danh mục học liệu để tra cứu và mượn thêm theo phong cách OPAC
    public List<BookItem> AvailableBooks { get; set; } = new();
}

public class BorrowTransactionViewModel
{
    public string MaGD { get; set; } = string.Empty;
    public string MaSach { get; set; } = string.Empty;
    public string TenSach { get; set; } = string.Empty;
    public string TacGia { get; set; } = string.Empty;
    public string CoverImage { get; set; } = string.Empty;
    public string NgayMuon { get; set; } = string.Empty;
    public string NgayHenTra { get; set; } = string.Empty;
    public string? NgayTra { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool IsQuaHan { get; set; }
    public int SoNgayQuaHan { get; set; }
    public decimal TienPhat { get; set; } = 0;
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
    public string TepDuLieu { get; set; } = "DanhSach_SinhVien_K65_CLCCNTT1.xlsx";
    public List<ImportQldtItemViewModel> PreviewItems { get; set; } = new();
    public int TotalValid => PreviewItems.Count(x => x.IsValid);
    public int TotalError => PreviewItems.Count(x => !x.IsValid);
}
