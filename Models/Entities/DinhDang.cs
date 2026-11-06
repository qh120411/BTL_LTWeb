using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class DinhDang
{
    public int MaDinhDang { get; set; }

    public string TenDinhDang { get; set; } = null!;

    public virtual ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
}
