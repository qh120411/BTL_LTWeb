using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class BanDoc
{
    public int MaBanDoc { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? DiaChi { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? MaLop { get; set; }

    public string? SoThe { get; set; }

    public DateOnly? NgayCapThe { get; set; }

    public DateOnly? NgayHetHanThe { get; set; }

    public string? TrangThaiThe { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public virtual ICollection<DatTruoc> DatTruocs { get; set; } = new List<DatTruoc>();

    public virtual Lop? MaLopNavigation { get; set; }

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();

    public virtual ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();

    public virtual ICollection<ViPham> ViPhams { get; set; } = new List<ViPham>();
}
