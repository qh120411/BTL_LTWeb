namespace BTL_LTWEB.Areas.Reader.Models
{
    public class BorrowedBookItem
    {
        public string MaGD { get; set; } = string.Empty;
        public string MaVach { get; set; } = string.Empty;
        public string TenSach { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; } // Quy định mượn luôn là 1 năm: NgayMuon.AddYears(1)
        public bool IsQuaHan { get; set; }
        public int SoNgayQuaHan { get; set; }
        public int SoNgayConLai { get; set; }
        public string TrangThaiText { get; set; } = "Đang mượn";
        public decimal TienPhat { get; set; } = 0; // Nếu quá hạn: 100.000 đ/quyển
    }

    public class UserProfileViewModel
    {
        // Thông tin định danh cơ bản (Read-only theo hồ sơ QLĐT / Trường)
        public string MaSinhVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string SoCCCD { get; set; } = string.Empty;
        public string NganhHoc { get; set; } = "CLC Công nghệ thông tin Việt - Anh";
        public string LopQuanLy { get; set; } = "CLC Công nghệ thông tin - Anh 1 K65";
        public string? AvatarUrl { get; set; }

        // Thông tin chỉnh sửa được (Người dùng tự nhập & cập nhật)
        public string? SoDienThoai { get; set; }
        public string? DiaChiNha { get; set; }
        public string? SoTaiKhoanNganHang { get; set; }
        public string? TenNganHang { get; set; }
        public string? Email { get; set; }

        // Danh sách sách đang mượn & Trạng thái mượn trả
        public List<BorrowedBookItem> BorrowedBooks { get; set; } = new();

        // Thống kê tổng hợp
        public int TongSoSachDangMuon => BorrowedBooks.Count;
        public int SoSachConHan => BorrowedBooks.Count(b => !b.IsQuaHan);
        public int SoSachQuaHan => BorrowedBooks.Count(b => b.IsQuaHan);
        public decimal TongTienPhaiThanhToan => BorrowedBooks.Sum(b => b.TienPhat);
    }
}
