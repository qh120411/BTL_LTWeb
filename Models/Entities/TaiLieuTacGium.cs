using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TaiLieuTacGium
{
    public int MaTaiLieu { get; set; }

    public int MaTacGia { get; set; }

    public string? VaiTroTacGia { get; set; }

    public virtual TacGium MaTacGiaNavigation { get; set; } = null!;

    public virtual TaiLieu MaTaiLieuNavigation { get; set; } = null!;
}
