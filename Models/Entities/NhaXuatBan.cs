using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class NhaXuatBan
{
    public int MaNhaXuatBan { get; set; }

    public string TenNhaXuatBan { get; set; } = null!;

    public string? QuocGia { get; set; }

    public string? DiaChi { get; set; }

    public string? SoDienThoai { get; set; }

    public virtual ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
}
