using BTL_LTWEB.Areas.Admin.Services;

namespace BTL_LTWEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register Business Services
            builder.Services.AddSingleton<IReaderService, ReaderService>();
            builder.Services.AddSingleton<IReportService, ReportService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();

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

            // Default Route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
