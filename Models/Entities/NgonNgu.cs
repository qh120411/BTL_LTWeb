using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class NgonNgu
{
    public string MaNgonNgu { get; set; } = null!;

    public string TenNgonNgu { get; set; } = null!;

    public virtual ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
}
