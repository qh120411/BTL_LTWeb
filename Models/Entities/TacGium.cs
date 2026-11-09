using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TacGium
{
    public int MaTacGia { get; set; }

    public string HoTen { get; set; } = null!;

    public string? QuocTich { get; set; }

    public virtual ICollection<TaiLieuTacGium> TaiLieuTacGia { get; set; } = new List<TaiLieuTacGium>();
}
