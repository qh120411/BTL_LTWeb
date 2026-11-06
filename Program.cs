using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using BTL_LTWEB.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<QLThuVienUtcContext>(o =>
{
    if (builder.Configuration["Database:Provider"] == "SqlServer")
        o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    else
        o.UseSqlite(builder.Configuration.GetConnectionString("DemoConnection"));
});
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBookCatalogService, BookCatalogService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.Cookie.Name = "UTC_Library_MVC";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(4);
    o.Events.OnValidatePrincipal = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<QLThuVienUtcContext>();
        if (!int.TryParse(context.Principal?.FindFirst("AccountId")?.Value, out var id) || !await db.TaiKhoans.AnyAsync(t => t.MaTaiKhoan == id && t.TrangThai == "Hoạt động"))
        {
<<<<<<< Updated upstream
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
=======
            context.RejectPrincipal();
            await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme);
>>>>>>> Stashed changes
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Chỉ khởi tạo database demo riêng; không sửa hoặc tự seed SQL Server của nhóm.
if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Database:SeedDemo") && builder.Configuration["Database:Provider"] == "Sqlite")
{
    using var scope = app.Services.CreateScope();
    DemoDatabase.Initialize(scope.ServiceProvider.GetRequiredService<QLThuVienUtcContext>());
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program
{
}
