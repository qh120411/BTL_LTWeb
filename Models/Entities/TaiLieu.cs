using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class TaiLieu
{
    public int MaTaiLieu { get; set; }

    public string? Isbn { get; set; }

    public string TenTaiLieu { get; set; } = null!;

    public int? MaNhaXuatBan { get; set; }

    public string? NgonNgu { get; set; }

    public int? MaTheLoai { get; set; }

    public string? DinhDang { get; set; }

    public short? NamXuatBan { get; set; }

    public int? LanXuatBan { get; set; }

    public int? SoTrang { get; set; }

    public string? MoTa { get; set; }

    public string? AnhBia { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ICollection<BanSao> BanSaos { get; set; } = new List<BanSao>();

    public virtual ICollection<DatTruoc> DatTruocs { get; set; } = new List<DatTruoc>();

    public virtual NhaXuatBan? MaNhaXuatBanNavigation { get; set; }

    public virtual TheLoai? MaTheLoaiNavigation { get; set; }

    public virtual ICollection<TaiLieuTacGium> TaiLieuTacGia { get; set; } = new List<TaiLieuTacGium>();
}
