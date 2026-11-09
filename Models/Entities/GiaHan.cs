using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class GiaHan
{
    public int MaGiaHan { get; set; }

    public int MaChiTietMuon { get; set; }

    public DateOnly HanTraCu { get; set; }

    public DateOnly HanTraMoi { get; set; }

    public int MaNhanVienDuyet { get; set; }

    public DateTime NgayGiaHan { get; set; }

    public string? LyDo { get; set; }

    public virtual ChiTietMuon MaChiTietMuonNavigation { get; set; } = null!;

    public virtual NhanVien MaNhanVienDuyetNavigation { get; set; } = null!;
}
