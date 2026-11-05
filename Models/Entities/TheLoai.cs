using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TheLoai
{
    public int MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public int? MaTheLoaiCha { get; set; }

    public virtual ICollection<TheLoai> InverseMaTheLoaiChaNavigation { get; set; } = new List<TheLoai>();

    public virtual TheLoai? MaTheLoaiChaNavigation { get; set; }

    public virtual ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
}
