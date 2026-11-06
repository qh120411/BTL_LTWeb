using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_LTWEB.Areas.Admin.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string? Roles { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "", returnUrl });
                return;
            }

            // Kiểm tra xem người dùng có phải nhân viên / cán bộ quản trị không
            bool isStaff = user.IsInRole("Admin") || 
                           user.IsInRole("ADMIN") || 
                           user.IsInRole("QUANLY_BANDOC") || 
                           user.IsInRole("QUANLY_TAILIEU") || 
                           user.IsInRole("Thuthu") ||
                           user.IsInRole("AdminStaff");

            // Sinh viên / Bạn đọc tuyệt đối không được vào bất kỳ trang quản trị nào
            if (!isStaff || user.IsInRole("Reader") || user.IsInRole("BANDOC"))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                return;
            }

            // Quản trị viên tối cao (ADMIN) có toàn bộ quyền
            if (user.IsInRole("Admin") || user.IsInRole("ADMIN"))
            {
                return;
            }

            // Nếu Action / Controller yêu cầu vai trò cụ thể:
            if (!string.IsNullOrWhiteSpace(Roles))
            {
                var allowedRoles = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                bool hasPermission = allowedRoles.Any(r => user.IsInRole(r));
                if (!hasPermission)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                }
            }
        }
    }
}
