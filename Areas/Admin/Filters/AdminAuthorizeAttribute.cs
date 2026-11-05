using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_LTWEB.Areas.Admin.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "", returnUrl });
                return;
            }

            // Nếu người dùng không có vai trò Admin hoặc Thuthu -> CHẶN TUYỆT ĐỐI!
            if (!user.IsInRole("Admin") && !user.IsInRole("Thuthu"))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
            }
        }
    }
}
