using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TienPhat
{
    public int MaTienPhat { get; set; }

    public int MaViPham { get; set; }

    public decimal SoTien { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChiTietThanhToan> ChiTietThanhToans { get; set; } = new List<ChiTietThanhToan>();

    public virtual ViPham MaViPhamNavigation { get; set; } = null!;
}
