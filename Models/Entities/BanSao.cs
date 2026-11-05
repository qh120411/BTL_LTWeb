using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class BanSao
{
    public int MaBanSao { get; set; }

    public int MaTaiLieu { get; set; }

    public string MaVach { get; set; } = null!;

    public string? ViTriKe { get; set; }

    public string TinhTrang { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public DateOnly? NgayNhap { get; set; }

    public virtual ICollection<ChiTietMuon> ChiTietMuons { get; set; } = new List<ChiTietMuon>();

    public virtual TaiLieu MaTaiLieuNavigation { get; set; } = null!;
}
