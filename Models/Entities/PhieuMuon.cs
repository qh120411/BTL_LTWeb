using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class PhieuMuon
{
    public int MaPhieuMuon { get; set; }

    public int MaTheBanDoc { get; set; }

    public int MaNhanVien { get; set; }

    public DateTime NgayMuon { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietMuon> ChiTietMuons { get; set; } = new List<ChiTietMuon>();

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;

    public virtual TheBanDoc MaTheBanDocNavigation { get; set; } = null!;
}
