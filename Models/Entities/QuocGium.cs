using System;
using System.Collections.Generic;

namespace BTL_LTWEB.Models.Entities;

public partial class QuocGium
{
    public string MaQuocGia { get; set; } = null!;

    public string TenQuocGia { get; set; } = null!;

    public virtual ICollection<NhaXuatBan> NhaXuatBans { get; set; } = new List<NhaXuatBan>();

    public virtual ICollection<TacGium> TacGia { get; set; } = new List<TacGium>();
}
