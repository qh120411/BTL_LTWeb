using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(options => options.Filters.Add<BTL_LTWEB.Common.SqlUnavailableFilter>());

            // Cấu hình quy tắc tìm kiếm View Engine phân cấp rõ ràng (Home có Admin, Reader, và Index đăng nhập)
            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                // Cho Controller thông thường
                options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/{1}/Admin/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/{1}/Reader/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/Admin/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/Reader/{0}.cshtml");

                // Cho Controller thuộc Area: {2} = Area (Admin/Reader), {1} = Controller, {0} = Action
                options.AreaViewLocationFormats.Add("/Views/{1}/{2}/{0}.cshtml"); // VD: /Views/Home/Admin/Index.cshtml
                options.AreaViewLocationFormats.Add("/Views/{2}/{1}/{0}.cshtml"); // VD: /Views/Admin/Catalog/Index.cshtml
                options.AreaViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Views/Shared/{2}/{0}.cshtml"); // VD: /Views/Shared/Admin/_AdminLayout.cshtml
                options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
            });

            // Đăng ký Entity Framework Core kết nối trực tiếp CSDL SQL Server QLThuVienUTC
            builder.Services.AddDbContext<QLThuVienUtcContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register Business Services (Scoped tương thích hoàn hảo với EF Core)
            builder.Services.AddScoped<IReaderService, ReaderService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IBookCatalogService, BookCatalogService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            // Cookie Authentication: Cookie name hoàn toàn mới để xóa sạch toàn bộ cookie cũ trong trình duyệt thường
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "UTC_Library_Auth_v3";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.SlidingExpiration = true;
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Kích hoạt Attribute Routing cho các API và trang tài liệu /ApiDoc
            app.MapControllers();

            // Cấu hình Routing chuẩn hóa theo 2 phân hệ Admin & Reader
            app.MapAreaControllerRoute(
                name: "admin_area",
                areaName: "Admin",
                pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapAreaControllerRoute(
                name: "reader_area",
                areaName: "Reader",
                pattern: "Reader/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Default Route -> Trang chủ công khai; chức năng cá nhân/quản trị tự yêu cầu đăng nhập.
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            
            app.Run();
        }
    }
}
