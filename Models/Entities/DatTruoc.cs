using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class DatTruoc
{
    public int MaDatTruoc { get; set; }

    public int MaBanDoc { get; set; }

    public int MaTaiLieu { get; set; }

    public DateTime NgayDat { get; set; }

    public DateTime? HanNhan { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<LichSuDatTruoc> LichSuDatTruocs { get; set; } = new List<LichSuDatTruoc>();

    public virtual BanDoc MaBanDocNavigation { get; set; } = null!;

    public virtual TaiLieu MaTaiLieuNavigation { get; set; } = null!;
}
