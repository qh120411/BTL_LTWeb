using System.ComponentModel.DataAnnotations;

namespace BTL_LTWEB.Areas.Reader.Models;

public class ProfileContactInputModel
{
    [StringLength(20)] public string? SoDienThoai { get; set; }
    [StringLength(255)] public string? DiaChiNha { get; set; }
    [StringLength(50)] public string? SoTaiKhoanNganHang { get; set; }
    [StringLength(150)] public string? TenNganHang { get; set; }
    [StringLength(150), EmailAddress] public string? Email { get; set; }
}
