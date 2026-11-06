using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class LichSuDatTruoc
{
    public int MaLichSu { get; set; }

    public int MaDatTruoc { get; set; }

    public string? TrangThaiCu { get; set; }

    public string TrangThaiMoi { get; set; } = null!;

    public DateTime ThoiGian { get; set; }

    public string? GhiChu { get; set; }

    public virtual DatTruoc MaDatTruocNavigation { get; set; } = null!;
}
