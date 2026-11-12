using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class NhanVien
{
    public int MaNhanVien { get; set; }

    public string? MaSoNhanVien { get; set; }

    public string HoTen { get; set; } = null!;

    public string? GioiTinh { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? ChucVu { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<GiaHan> GiaHans { get; set; } = new List<GiaHan>();

    public virtual ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();

    public virtual ICollection<ViPham> ViPhams { get; set; } = new List<ViPham>();
}
