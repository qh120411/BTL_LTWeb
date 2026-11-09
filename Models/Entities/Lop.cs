using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class Lop
{
    public string MaLop { get; set; } = null!;

    public string TenLop { get; set; } = null!;

    public string? KhoaHoc { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<BanDoc> BanDocs { get; set; } = new List<BanDoc>();
}
