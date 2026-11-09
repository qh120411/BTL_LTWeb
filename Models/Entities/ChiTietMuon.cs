using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class ChiTietMuon
{
    public int MaChiTietMuon { get; set; }

    public int MaPhieuMuon { get; set; }

    public int MaBanSao { get; set; }

    public DateOnly HanTra { get; set; }

    public DateTime? NgayTra { get; set; }

    public string? TinhTrangKhiTra { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<GiaHan> GiaHans { get; set; } = new List<GiaHan>();

    public virtual BanSao MaBanSaoNavigation { get; set; } = null!;

    public virtual PhieuMuon MaPhieuMuonNavigation { get; set; } = null!;

    public virtual ICollection<ViPham> ViPhams { get; set; } = new List<ViPham>();
}
