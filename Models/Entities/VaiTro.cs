using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class VaiTro
{
    public string MaVaiTro { get; set; } = null!;

    public string TenVaiTro { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Quyen> MaQuyens { get; set; } = new List<Quyen>();

    public virtual ICollection<TaiKhoan> MaTaiKhoans { get; set; } = new List<TaiKhoan>();
}
