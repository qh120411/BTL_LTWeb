namespace BTL_LTWEB.Areas.Admin.ViewModels
{
    public class StaffAccountItemViewModel
    {
        public int MaTaiKhoan { get; set; }
        public string MaNV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTroCode { get; set; } = string.Empty;
        public string VaiTroText { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Hoạt động";
        public bool IsCurrentLoggedInUser { get; set; }
        public bool IsSuperAdminAccount { get; set; }
        public bool CanEdit { get; set; }
    }

    public class UserManagementIndexViewModel
    {
        public List<StaffAccountItemViewModel> StaffList { get; set; } = new();
        public string? Keyword { get; set; }
        public string? SelectedRole { get; set; }
        public bool IsCurrentUserSuperAdmin { get; set; }
        public string CurrentUsername { get; set; } = string.Empty;
    }
}
