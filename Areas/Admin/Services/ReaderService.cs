using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;

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
    public string HieuLucQuyen { get; set; } = "2027-06-30";
    public int SoSachDangMuon { get; set; }
    public int SoSachQuaHan { get; set; }
    public List<BorrowTransactionViewModel> BorrowTransactions { get; set; } = new();
}

public class ReaderService : IReaderService
{
    private static readonly List<ReaderRecord> _readers = new()
    {
        new ReaderRecord
        {
            MaBD = "SV001",
            HoTen = "Nguyễn Minh Anh",
            LopKhoa = "CNTT · K65",
            Nhom = "Sinh viên",
            TrangThaiHocTap = "Đang học",
            QuyenSuDung = "Hoạt động",
            GioiHanSach = "5 cuốn (giá trị minh họa)",
            HieuLucQuyen = "2027-06-30",
            SoSachDangMuon = 2,
            SoSachQuaHan = 1,
            BorrowTransactions = new List<BorrowTransactionViewModel>
            {
                new()
                {
                    MaGD = "GD2610270001",
                    MaSach = "TLVN26-001-001",
                    TenSach = "Nhập môn lập trình Python",
                    NgayMuon = "15/10/2026",
                    NgayHenTra = "29/10/2026",
                    TrangThai = "Quá hạn 5 ngày",
                    IsQuaHan = true
                },
                new()
                {
                    MaGD = "GD2610270002",
                    MaSach = "TLVN26-002-001",
                    TenSach = "Cơ sở dữ liệu nâng cao",
                    NgayMuon = "25/10/2026",
                    NgayHenTra = "08/11/2026",
                    TrangThai = "Đang mượn",
                    IsQuaHan = false
                }
            }
        },
        new ReaderRecord
        {
            MaBD = "SV002",
            HoTen = "Trần Hoàng Nam",
            LopKhoa = "CNTT · K65",
            Nhom = "Sinh viên",
            TrangThaiHocTap = "Đang học",
            QuyenSuDung = "Hoạt động",
            GioiHanSach = "5 cuốn",
            HieuLucQuyen = "2027-06-30",
            SoSachDangMuon = 1,
            SoSachQuaHan = 0,
            BorrowTransactions = new List<BorrowTransactionViewModel>
            {
                new()
                {
                    MaGD = "GD2610100003",
                    MaSach = "TLVN26-001-002",
                    TenSach = "Nhập môn lập trình Python",
                    NgayMuon = "10/10/2026",
                    NgayHenTra = "24/10/2026",
                    NgayTra = "20/10/2026",
                    TrangThai = "Đã trả",
                    IsQuaHan = false
                }
            }
        },
        new ReaderRecord
        {
            MaBD = "GV001",
            HoTen = "Lê Thu Hà",
            LopKhoa = "Khoa CNTT",
            Nhom = "Giảng viên",
            TrangThaiHocTap = "Đang công tác",
            QuyenSuDung = "Hoạt động",
            GioiHanSach = "10 cuốn",
            HieuLucQuyen = "2028-12-31",
            SoSachDangMuon = 3,
            SoSachQuaHan = 0,
            BorrowTransactions = new List<BorrowTransactionViewModel>()
        },
        new ReaderRecord
        {
            MaBD = "NG001",
            HoTen = "Nguyễn Hoàng An",
            LopKhoa = "Độc giả tự do",
            Nhom = "Ngoài trường",
            TrangThaiHocTap = "Ngoài trường",
            QuyenSuDung = "Đọc tại chỗ",
            GioiHanSach = "2 cuốn",
            HieuLucQuyen = "2026-12-31",
            SoSachDangMuon = 0,
            SoSachQuaHan = 0,
            BorrowTransactions = new List<BorrowTransactionViewModel>()
        }
    };

    public ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup)
    {
        var query = _readers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(selectedGroup) && selectedGroup != "Tất cả nhóm")
        {
            query = query.Where(r => r.Nhom.Equals(selectedGroup, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var kw = searchKeyword.Trim().ToLower();
            query = query.Where(r => r.MaBD.ToLower().Contains(kw) ||
                                     r.HoTen.ToLower().Contains(kw) ||
                                     r.LopKhoa.ToLower().Contains(kw));
        }

        var list = query.Select(r => new ReaderListItemViewModel
        {
            MaBD = r.MaBD,
            HoTen = r.HoTen,
            Nhom = r.Nhom,
            LopKhoa = r.LopKhoa,
            QuyenSuDung = r.QuyenSuDung,
            SoSachDangMuon = r.SoSachDangMuon,
            SoSachQuaHan = r.SoSachQuaHan
        }).ToList();

        return new ReaderIndexViewModel
        {
            SearchKeyword = searchKeyword,
            SelectedGroup = selectedGroup ?? "Tất cả nhóm",
            Readers = list
        };
    }

    public ReaderDetailViewModel? GetReaderById(string id)
    {
        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (reader == null) return null;

        return new ReaderDetailViewModel
        {
            MaBD = reader.MaBD,
            HoTen = reader.HoTen,
            LopKhoa = reader.LopKhoa,
            TrangThaiHocTap = reader.TrangThaiHocTap,
            Nhom = reader.Nhom,
            QuyenSuDung = reader.QuyenSuDung,
            GioiHanSach = reader.GioiHanSach,
            HieuLucQuyen = reader.HieuLucQuyen,
            SoSachDangMuon = reader.SoSachDangMuon,
            SoSachQuaHan = reader.SoSachQuaHan,
            BorrowTransactions = reader.BorrowTransactions
        };
    }

    public bool UpdateReaderSettings(ReaderDetailViewModel model)
    {
        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(model.MaBD, StringComparison.OrdinalIgnoreCase));
        if (reader == null) return false;

        reader.Nhom = model.Nhom ?? reader.Nhom;
        reader.QuyenSuDung = model.QuyenSuDung ?? reader.QuyenSuDung;
        reader.GioiHanSach = model.GioiHanSach ?? reader.GioiHanSach;
        reader.HieuLucQuyen = model.HieuLucQuyen ?? reader.HieuLucQuyen;
        return true;
    }

    public ImportQldtViewModel GetImportPreview()
    {
        return new ImportQldtViewModel
        {
            NguonDuLieu = "Danh sách sinh viên từ QLĐT",
            TepDuLieu = "sinh-vien-k65.xlsx (minh họa)",
            PreviewItems = new List<ImportQldtItemViewModel>
            {
                new() { MaSV = "SV001", HoTen = "Nguyễn Minh Anh", LopKhoa = "CNTT · K65", TrangThaiKiemTra = "Hợp lệ / cập nhật", IsNew = false, IsValid = true },
                new() { MaSV = "SV002", HoTen = "Trần Hoàng Nam", LopKhoa = "CNTT · K65", TrangThaiKiemTra = "Hợp lệ / cập nhật", IsNew = false, IsValid = true },
                new() { MaSV = "SV003", HoTen = "Đỗ Ngọc Linh", LopKhoa = "CNTT · K65", TrangThaiKiemTra = "Hợp lệ / thêm mới", IsNew = true, IsValid = true }
            }
        };
    }

    public bool ProcessImport(out int updatedCount, out int insertedCount)
    {
        updatedCount = 2;
        insertedCount = 1;

        // Cập nhật SV001, SV002
        var sv1 = _readers.FirstOrDefault(r => r.MaBD == "SV001");
        if (sv1 != null) { sv1.HoTen = "Nguyễn Minh Anh"; sv1.LopKhoa = "CNTT · K65"; }

        var sv2 = _readers.FirstOrDefault(r => r.MaBD == "SV002");
        if (sv2 != null) { sv2.HoTen = "Trần Hoàng Nam"; sv2.LopKhoa = "CNTT · K65"; }

        // Thêm SV003 nếu chưa có
        if (!_readers.Any(r => r.MaBD == "SV003"))
        {
            _readers.Add(new ReaderRecord
            {
                MaBD = "SV003",
                HoTen = "Đỗ Ngọc Linh",
                LopKhoa = "CNTT · K65",
                Nhom = "Sinh viên",
                TrangThaiHocTap = "Đang học",
                QuyenSuDung = "Hoạt động",
                GioiHanSach = "5 cuốn",
                HieuLucQuyen = "2027-06-30",
                SoSachDangMuon = 0,
                SoSachQuaHan = 0,
                BorrowTransactions = new List<BorrowTransactionViewModel>()
            });
        }

        return true;
    }
}

