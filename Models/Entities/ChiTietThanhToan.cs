using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class ChiTietThanhToan
{
    public int MaChiTietThanhToan { get; set; }

    public int MaThanhToan { get; set; }

    public int MaTienPhat { get; set; }

    public decimal SoTien { get; set; }

    public virtual ThanhToan MaThanhToanNavigation { get; set; } = null!;

    public virtual TienPhat MaTienPhatNavigation { get; set; } = null!;
}
