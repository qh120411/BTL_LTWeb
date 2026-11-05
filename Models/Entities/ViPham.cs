using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class ViPham
{
    public int MaViPham { get; set; }

    public int MaBanDoc { get; set; }

    public int? MaChiTietMuon { get; set; }

    public string LoaiViPham { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime NgayViPham { get; set; }

    public virtual BanDoc MaBanDocNavigation { get; set; } = null!;

    public virtual ChiTietMuon? MaChiTietMuonNavigation { get; set; }

    public virtual TienPhat? TienPhat { get; set; }
}
