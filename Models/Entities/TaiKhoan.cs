using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string? EmailDangNhap { get; set; }

    public string MatKhauHash { get; set; } = null!;

    public int? MaBanDoc { get; set; }

    public int? MaNhanVien { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public virtual BanDoc? MaBanDocNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual ICollection<VaiTro> MaVaiTros { get; set; } = new List<VaiTro>();
}
