using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Đăng ký Entity Framework Core kết nối trực tiếp CSDL SQL Server QLThuVienUTC
            builder.Services.AddDbContext<QLThuVienUtcContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register Business Services (Scoped tương thích hoàn hảo với EF Core)
            builder.Services.AddScoped<IReaderService, ReaderService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IBookCatalogService, BookCatalogService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            // Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
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

            // Cấu hình Routing chuẩn hóa theo README.md cho 2 phân hệ Admin & Reader
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

            // Default Route -> Đăng nhập
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
