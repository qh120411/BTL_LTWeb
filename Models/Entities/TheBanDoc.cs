using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TheBanDoc
{
    public int MaTheBanDoc { get; set; }

    public int MaBanDoc { get; set; }

    public string SoThe { get; set; } = null!;

    public DateOnly NgayCap { get; set; }

    public DateOnly? NgayHetHan { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual BanDoc MaBanDocNavigation { get; set; } = null!;

    public virtual ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();
}
