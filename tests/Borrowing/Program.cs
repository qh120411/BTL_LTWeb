using System.Text.Json;
using BTL_LTWEB.Common;
using BTL_LTWEB.Areas.Admin.Controllers;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc;
using BTL_LTWEB.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore.Diagnostics;

var databaseName = "QLThuVienUTC_BorrowCheck_" + Guid.NewGuid().ToString("N")[..8];
var options = new DbContextOptionsBuilder<QLThuVienUtcContext>()
    .UseSqlServer($"Server=.\\HUYPC;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=10")
    .Options;
using var db = new QLThuVienUtcContext(options);
var root = Path.Combine(AppContext.BaseDirectory, "TestData-" + Guid.NewGuid().ToString("N")[..8]);
Directory.CreateDirectory(Path.Combine(root, "App_Data"));
var env = new TestEnvironment { ContentRootPath = root };
var seedBooks = Enumerable.Range(1, 15).Select(i => new BookItem
{
    Id = 100 + i,
    Title = $"Verification Book {i}",
    Authors = "Verification Author",
    Publisher = "Verification Publisher",
    ISBN = $"97800000000{i:D2}",
    TotalCopies = 2,
    AvailableCopies = 2,
    Shelf = "Verification Shelf"
}).ToList();
seedBooks.Add(new BookItem { Id = 201, Title = "No Stock", AvailableCopies = 0, TotalCopies = 2 });
seedBooks.Add(new BookItem { Id = 202, Title = "Online Only", IsOnline = true });
File.WriteAllText(Path.Combine(root, "App_Data", "catalog_books.json"), JsonSerializer.Serialize(seedBooks));

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

Check(AccountStatus.IsActive("Hoạt động") && AccountStatus.IsActive("Hoáº¡t Ä‘á»™ng") &&
      !AccountStatus.IsActive("Tạm khóa") && !AccountStatus.IsActive(null),
    "Account status must handle the known legacy encoding without accepting locked/unknown states");

var semesterCases = new[]
{
    (new DateOnly(2026, 1, 2), new DateOnly(2026, 5, 31)),
    (new DateOnly(2026, 5, 30), new DateOnly(2026, 5, 31)),
    (new DateOnly(2026, 5, 31), new DateOnly(2026, 5, 31)),
    (new DateOnly(2026, 8, 11), new DateOnly(2026, 12, 31)),
    (new DateOnly(2026, 8, 31), new DateOnly(2026, 12, 31)),
    (new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 31)),
    (new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 31)),
    (new DateOnly(2027, 1, 2), new DateOnly(2027, 5, 31)),
    (new DateOnly(2028, 2, 29), new DateOnly(2028, 5, 31))
};
foreach (var (borrowedOn, expectedDueDate) in semesterCases)
    Check(LoanTermPolicy.GetDueDate(borrowedOn) == expectedDueDate, $"Wrong semester due date for {borrowedOn}");
var outsideSemesterDates = new[]
{
    new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 1), new DateOnly(2026, 7, 15),
    new DateOnly(2026, 8, 10), new DateOnly(2027, 1, 1)
};
foreach (var borrowedOn in outsideSemesterDates)
    Check(!LoanTermPolicy.TryGetDueDate(borrowedOn, out _), $"Borrowing should be closed on {borrowedOn}");
Console.WriteLine("PASS: exact 02/01–31/05 and 11/08–31/12 semester windows, inclusive end dates, year rollover and leap day.");

try
{
    Check(databaseName.StartsWith("QLThuVienUTC_BorrowCheck_"), "Invalid disposable database name");
    db.Database.EnsureCreated();
    // Keep staff empty but advance its identity to catch hard-coded employee IDs.
    var removedStaff = new NhanVien { HoTen = "Temporary Staff", MaSoNhanVien = "REMOVED", TrangThai = "Hoạt động" };
    db.NhanViens.Add(removedStaff);
    db.SaveChanges();
    db.NhanViens.Remove(removedStaff);
    db.SaveChanges();
    var student = new BanDoc { MaSinhVien = "TESTSV1", HoTen = "Test Student", TrangThai = "Hoạt động" };
    var other = new BanDoc { MaSinhVien = "TESTSV2", HoTen = "Other Student", TrangThai = "Hoạt động" };
    var lecturer = new BanDoc { MaSinhVien = "GVTEST01", HoTen = "Test Lecturer", TrangThai = "Hoạt động" };
    db.BanDocs.AddRange(student, other, lecturer);
    db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "admin", MatKhauHash = "verification-only", VaiTro = "ADMIN", TrangThai = "Hoạt động" });
    db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "reader", MatKhauHash = "verification-only", VaiTro = "READER", TrangThai = "Hoạt động", MaBanDocNavigation = other });
    db.SaveChanges();
    Check(db.TaiLieus.Count() == 0 && db.NhanViens.Count() == 0, "Test must reproduce the empty SQL inventory/staff");

    var catalog = new BookCatalogService(env);
    var profiles = new UserProfileService(env, db);
    var today = DateOnly.FromDateTime(DateTime.Today);
    var testDay = LoanTermPolicy.TryGetDueDate(today, out _) ? today :
        today.Month == 1 ? new DateOnly(today.Year, 1, 2) : new DateOnly(today.Year, 8, 11);
    var service = new ReaderService(db, catalog, profiles, env, new FixedBorrowingClock(testDay));
    bool Borrow(string reader, int id, out string error) => service.BorrowBook(reader, id, out _, out error, "admin");

    db.BanDocs.AddRange(Enumerable.Range(0, 16).Select(i => new BanDoc
    {
        MaSinhVien = $"SEARCH{i:D2}", HoTen = $"Đỗ Văn Đạt {i:D2}", TrangThai = "Hoạt động"
    }));
    db.SaveChanges();
    Check((await service.SearchBorrowReadersAsync(null)).Count == 0 &&
          (await service.SearchBorrowReadersAsync("a")).Count == 0 &&
          (await service.SearchBorrowReadersAsync(new string('a', 101))).Count == 0,
        "Reader search must reject blank, too short and too long queries");
    var nameMatches = await service.SearchBorrowReadersAsync("  do van dat  ");
    Check(nameMatches.Count == 10 && nameMatches.All(r => r.HoTen.StartsWith("Đỗ Văn Đạt")),
        "Reader search must support unaccented Vietnamese names and limit to ten results");
    Check((await service.SearchBorrowReadersAsync("ĐỖ VĂN ĐẠT")).Count == 10,
        "Reader search must support accented names and ignore letter case");
    Check((await service.SearchBorrowReadersAsync("SEARCH15")).First().MaBD == "SEARCH15",
        "Reader search must prioritize the exact reader code even beyond the first ten names");
    Check((await service.SearchBorrowReadersAsync("no-match-xyz")).Count == 0,
        "Unknown reader search must return an empty list");
    Console.WriteLine("PASS: bounded SQL reader search, Vietnamese names with/without accents, exact codes and input limits.");

    foreach (var closedDay in outsideSemesterDates)
    {
        var clock = new FixedBorrowingClock(closedDay);
        var closedService = new ReaderService(db, catalog, profiles, env, clock);
        Check(!closedService.BorrowBook("TESTSV1", 101, out _, out var closedError, "admin") &&
              closedError == LoanTermPolicy.BorrowingWindowMessage, "SQL borrowing must reject out-of-semester dates");
        var api = new ApiDocController(db, catalog, closedService, profiles, clock);
        Check(api.BorrowBook(new ApiDocController.BorrowReq { MaBD = "TESTSV1", BookId = 101 }) is BadRequestObjectResult,
            "API must reject out-of-semester dates before its JSON fallback");
        Check(!db.TaiLieus.Any() && !db.PhieuMuons.Any() && !db.NhanViens.Any(), "Closed semester cannot create SQL records");
        Check(!File.Exists(Path.Combine(root, "App_Data", "student_profiles.json")), "Closed semester cannot create fallback profiles");
    }
    Console.WriteLine("PASS: out-of-semester SQL/API requests create neither inventory nor loans nor fallback profiles.");

    var controller = new CirculationController(service, catalog, db);
    var index = (ViewResult)controller.Index(null, null);
    Check(((CirculationIndexViewModel)index.Model!).AvailableBooks.Single(b => b.Id == 101).AvailableCopies == 2,
        "Empty SQL inventory must not disable available catalog titles in the borrow form");

    Check(Borrow("TESTSV1", 101, out var error), "First borrow: " + error);
    Check(db.TaiLieus.Count() == 1 && db.BanSaos.Count() == 2, "Only the selected catalog title should be initialized");
    Check(db.NhanViens.Single().MaNhanVien != 1, "Staff identity should differ from the old hard-coded value");
    Check(db.PhieuMuons.Single().MaNhanVien == db.NhanViens.Single().MaNhanVien, "Loan must reference the actual authenticated staff");
    var storedLoan = db.PhieuMuons.Single();
    var expectedDeadline = LoanTermPolicy.GetDueDate(DateOnly.FromDateTime(storedLoan.NgayMuon));
    Check(db.ChiTietMuons.Single().HanTra == expectedDeadline, "SQL must store the semester deadline");
    Check(db.TaiLieus.Single().MaTaiLieu != 101, "SQL and catalog IDs must differ in this test");
    Check(catalog.GetById(101)!.AvailableCopies == 1, "Catalog stock must decrease after SQL commit");
    Check(profiles.GetProfile("TESTSV1").BorrowedBooks.Count == 1, "Reader profile must show the loan");
    Check(DateOnly.FromDateTime(profiles.GetProfile("TESTSV1").BorrowedBooks.Single().HanTra) == expectedDeadline,
        "Reader profile must display the same deadline as SQL");
    Check(!Borrow("TESTSV1", 101, out error) && error.Contains("đã mượn đầu sách"), "Duplicate title must be rejected");
    var selectedReader = (await service.SearchBorrowReadersAsync("TESTSV1")).Single();
    Check(selectedReader.SoSachDangMuon == 1 && selectedReader.GioiHanSach == 5 &&
          selectedReader.BorrowedTitles.SequenceEqual(new[] { "Verification Book 1" }),
        "Search results must contain only this reader's active loan count and borrowed titles");
    Check(Borrow("TESTSV2", 101, out error), "Second copy: " + error);
    Check(!Borrow("GVTEST01", 101, out error) && error.Contains("không còn bản"), "No stock must be rejected without inventing a copy");
    Check(db.BanSaos.Count() == 2, "Out-of-stock request must not create extra copies");
    var loan = service.GetAllCirculationTransactions().Single(t => t.MaBD == "TESTSV1");
    Check(!service.ReturnBook("TESTSV2", loan.MaGD, out _, out _), "Cannot return another reader's transaction");
    Check(!service.ReturnBook("TESTSV1", "missing-transaction", out _, out _), "Invalid transaction cannot fall back to an arbitrary loan");
    Check(!service.ProcessReturnOrCompensation("TESTSV1", loan.MaGD, "Unknown", "Cash", 150000, true, null, out _),
        "Unknown return action cannot modify stock or payment");
    var firstDetail = db.ChiTietMuons.Single(c => c.MaPhieuMuon == storedLoan.MaPhieuMuon);
    firstDetail.HanTra = testDay.AddDays(-1);
    db.SaveChanges();
    var receiptsBeforeFailure = service.GetAllFinancialReceipts(null, null).Count;
    using (var failingDb = new QLThuVienUtcContext(new DbContextOptionsBuilder<QLThuVienUtcContext>()
        .UseSqlServer(db.Database.GetConnectionString()!).AddInterceptors(new FailSavingInterceptor()).Options))
    {
        var failingService = new ReaderService(failingDb, catalog, profiles, env, new FixedBorrowingClock(testDay));
        Check(!failingService.ProcessReturnOrCompensation("TESTSV1", loan.MaGD, "NormalReturn", "NewBook", 0, true, null, out _, "admin"),
            "Simulated SQL failure must reject the return");
    }
    Check(db.ChiTietMuons.AsNoTracking().Single(c => c.MaChiTietMuon == firstDetail.MaChiTietMuon).NgayTra == null &&
          service.GetAllFinancialReceipts(null, null).Count == receiptsBeforeFailure,
        "Failed return must leave loan and receipts unchanged");
    Check(service.ReturnBook("TESTSV1", loan.MaGD, out _, out _), "Returning the created loan must work");
    Check(db.ThanhToans.Single().TongTien == 100000m &&
          db.ThanhToans.Single().MaNhanVien == db.NhanViens.Single().MaNhanVien,
        "Overdue payment must use an existing staff identity rather than hard-coded ID 1");
    Check(!service.ReturnBook("TESTSV1", loan.MaGD, out _, out _) && db.ThanhToans.Count() == 1,
        "Repeated return must not charge twice");
    Check(catalog.GetById(101)!.AvailableCopies == 1, "Return must update the correct catalog title despite different SQL ID");
    Check(profiles.GetProfile("TESTSV1").BorrowedBooks.Count == 0, "Returned loan must disappear from profile");
    Check((await service.SearchBorrowReadersAsync("TESTSV1")).Single().SoSachDangMuon == 0,
        "Returned books must not count against the selected reader");
    Check((await service.SearchBorrowReadersAsync("GVTEST01")).Single().GioiHanSach == 10,
        "Reader search must return the lecturer quota");

    for (var i = 102; i <= 106; i++) Check(Borrow("TESTSV1", i, out error), $"Student loan {i}: {error}");
    Check(!Borrow("TESTSV1", 107, out error) && error.Contains("(5 cuốn)"), "Student quota must be five");
    for (var i = 102; i <= 111; i++) Check(Borrow("GVTEST01", i, out error), $"Lecturer loan {i}: {error}");
    Check(!Borrow("GVTEST01", 112, out error) && error.Contains("(10 cuốn)"), "Lecturer quota must be ten");
    Check(!Borrow("TESTSV2", 201, out _), "Empty catalog stock must be rejected");
    Check(!Borrow("TESTSV2", 202, out _), "Digital-only title must be rejected");
    Check(!Borrow("UNKNOWN", 113, out _), "Unknown reader must be rejected");
    Check(!service.BorrowBook("TESTSV2", 113, out _, out _, "reader"), "Reader account cannot initialize a staff record");
    Check(!db.TaiLieus.Any(d => d.TenTaiLieu == "Verification Book 13"), "Rejected request must not initialize inventory");

    var apiWithStaff = new ApiDocController(db, catalog, service, profiles, new FixedBorrowingClock(testDay))
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin"), new Claim(ClaimTypes.Role, "ADMIN") }, "test"))
        } }
    };
    Check(apiWithStaff.BorrowBook(new ApiDocController.BorrowReq { MaBD = "TESTSV2", BookId = 202 }) is BadRequestObjectResult,
        "API cannot bypass digital-only restrictions through JSON fallback");
    Check(apiWithStaff.BorrowBook(new ApiDocController.BorrowReq { MaBD = "UNKNOWN", BookId = 113 }) is BadRequestObjectResult,
        "API cannot create an offline loan for an unknown reader");
    Check(apiWithStaff.ReturnBook(new ApiDocController.ReturnReq { MaBD = "TESTSV2" }) is BadRequestObjectResult,
        "API return must require an explicit transaction");
    Check(apiWithStaff.ReturnBook(new ApiDocController.ReturnReq { MaBD = "TESTSV2", MaGD = "does-not-exist" }) is BadRequestObjectResult,
        "API cannot return the first profile book for an invalid transaction");

    var cashLoan = service.GetAllCirculationTransactions().Single(t => t.MaBD == "TESTSV2");
    Check(!service.ProcessReturnOrCompensation("TESTSV2", cashLoan.MaGD, "Compensation", "Cash", 0, true, null, out _, "admin"),
        "Zero cash compensation cannot silently become a different amount");
    Check(service.ProcessReturnOrCompensation("TESTSV2", cashLoan.MaGD, "Compensation", "Cash", 250000, false, null, out _, "admin"),
        "Cash compensation must close the selected loan");
    Check(db.ThanhToans.Count() == 2 && db.ThanhToans.OrderBy(t => t.MaThanhToan).Last().TongTien == 250000m,
        "Cash compensation must charge the entered amount exactly once");

    var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
        new Claim(ClaimTypes.NameIdentifier, "TESTSV1"), new Claim(ClaimTypes.Name, "Test Student") }, "test")) };
    var offlineProfiles = new UserProfileService(env);
    var profileController = new BTL_LTWEB.Areas.Reader.Controllers.ProfileController(offlineProfiles)
    {
        ControllerContext = new ControllerContext { HttpContext = context },
        TempData = new TempDataDictionary(context, new EmptyTempDataProvider())
    };
    var originalProfile = offlineProfiles.GetProfile("TESTSV1");
    var loanIds = originalProfile.BorrowedBooks.Select(b => b.MaGD).ToList();
    profileController.Index(new UserProfileViewModel { MaSinhVien = "TESTSV2", HoTen = "Forged name", SoDienThoai = "0123456789" });
    var updatedProfile = offlineProfiles.GetProfile("TESTSV1");
    Check(updatedProfile.HoTen == "Test Student" && updatedProfile.BorrowedBooks.Select(b => b.MaGD).SequenceEqual(loanIds) &&
          updatedProfile.SoDienThoai == "0123456789", "Contact form must preserve identity and existing loans even offline");
    Console.WriteLine("PASS: strict returns, actual payment staff, duplicate prevention, atomic SQL failure, API restrictions and profile preservation.");

    db.TaiLieus.Add(new TaiLieu
    {
        TenTaiLieu = "Existing foreign title",
        BanSaos = new List<BanSao> { new() { MaVach = "UTC-114-01", TrangThai = "Có sẵn", TinhTrang = "Tốt" } }
    });
    db.SaveChanges();
    var titleCount = db.TaiLieus.Count();
    var copyCount = db.BanSaos.Count();
    var loanCount = db.PhieuMuons.Count();
    Check(!Borrow("TESTSV2", 114, out _), "Barcode conflict must fail atomically");
    Check(db.TaiLieus.Count() == titleCount && db.BanSaos.Count() == copyCount && db.PhieuMuons.Count() == loanCount,
        "Failed borrow must roll back title, copies and loan together");

    var accountController = new AccountController(db);
    var originalHash = db.TaiKhoans.Single(t => t.TenDangNhap == "admin").MatKhauHash;
    Check(await accountController.Login("admin", "123456", null) is ViewResult &&
          db.TaiKhoans.Single(t => t.TenDangNhap == "admin").MatKhauHash == originalHash,
        "Generic fallback passwords must not authenticate or rewrite the stored admin password");
    db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "LOCKED", MatKhauHash = PasswordHelper.HashPassword("LockedTestPassword"),
        VaiTro = "ADMIN", TrangThai = "Tạm khóa" });
    db.SaveChanges();
    Check(await accountController.Login("LOCKED", "LockedTestPassword", null) is ViewResult,
        "Even a correct password must not authenticate a locked account");
    var staffContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
        new Claim(ClaimTypes.NameIdentifier, "admin"), new Claim(ClaimTypes.Role, "ADMIN") }, "test")) };
    var userController = new UserController(db) { ControllerContext = new ControllerContext { HttpContext = staffContext } };
    var staffCount = db.NhanViens.Count();
    await userController.AddStaff("NVVERIFY", "Test Staff", "staff@example.test", "INVALID_ROLE");
    Check(db.NhanViens.Count() == staffCount, "Invalid role cannot create an orphan staff record");
    var addStaffResult = (JsonResult)await userController.AddStaff("NVVERIFY", "Test Staff", "staff@example.test", "QUANLY_TAILIEU");
    Check(JsonSerializer.SerializeToElement(addStaffResult.Value).GetProperty("success").GetBoolean() &&
          db.TaiKhoans.Include(t => t.MaNhanVienNavigation).Single(t => t.TenDangNhap == "NVVERIFY").MaNhanVienNavigation?.MaSoNhanVien == "NVVERIFY",
        "Staff and its login account must be created together with the requested valid role");
    Console.WriteLine("PASS: locked accounts, removed password bypass, role validation and atomic staff/account creation.");
    Console.WriteLine("PASS: first borrow with empty inventory/staff, catalog-to-SQL mapping, reader profile and return flow.");
    Console.WriteLine("PASS: duplicate/out-of-stock/digital/invalid-reader checks, 5/10 quotas and rollback on SQL failure.");
}
finally
{
    if (!databaseName.StartsWith("QLThuVienUTC_BorrowCheck_")) throw new InvalidOperationException("Refusing cleanup outside disposable database");
    db.Database.EnsureDeleted();
    Console.WriteLine("Removed disposable verification database: " + databaseName);
    var outputRoot = Path.GetFullPath(AppContext.BaseDirectory);
    if (Path.GetFullPath(root).StartsWith(outputRoot, StringComparison.OrdinalIgnoreCase) && Directory.Exists(root))
        Directory.Delete(root, recursive: true);
}

sealed class TestEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "BorrowVerification";
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = "";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = "";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}

sealed class FixedBorrowingClock(DateOnly date) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new DateTimeOffset(date.ToDateTime(new TimeOnly(12, 0))).ToUniversalTime();
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;
}

sealed class FailSavingInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        => throw new InvalidOperationException("Simulated SQL save failure");
}

sealed class EmptyTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
