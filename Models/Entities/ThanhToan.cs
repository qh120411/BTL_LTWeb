using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class ThanhToan
{
    public int? MaChiTietMuon { get; set; }
    public decimal? TienNopMuon { get; set; }
    public decimal? TienDenBu { get; set; }
    public string? LoaiKhoanThu { get; set; }
    public string? HinhThucXuLy { get; set; }
    public string? GhiChu { get; set; }

    public int MaThanhToan { get; set; }

    public int MaBanDoc { get; set; }

    public int MaNhanVien { get; set; }

    public DateTime NgayThanhToan { get; set; }

    public decimal TongTien { get; set; }

    public string PhuongThuc { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public string? NoiDung { get; set; }

    public virtual BanDoc MaBanDocNavigation { get; set; } = null!;

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
