using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_LTWEB.Common;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiStaffAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Vui lòng đăng nhập tài khoản cán bộ." });
            return;
        }
        var roles = new[] { "ADMIN", "Admin", "QUANLY_BANDOC", "QUANLY_TAILIEU", "Thuthu", "THUTHU", "AdminStaff" };
        if (!roles.Any(user.IsInRole) || user.IsInRole("Reader") || user.IsInRole("READER") || user.IsInRole("BANDOC"))
            context.Result = new ObjectResult(new { message = "Tài khoản không có quyền thực hiện thao tác này." }) { StatusCode = 403 };
    }
}
