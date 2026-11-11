using System.Net;
using System.Text;
using BTL_LTWEB.Areas.Admin.Controllers;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static QLThuVienUtcContext Connect(string name) => new(new DbContextOptionsBuilder<QLThuVienUtcContext>()
    .UseSqlServer($"Server=.\\HUYPC;Database={name};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3")
    .Options);

foreach (var current in new[] { 1, 2, 5, 10, 18, 20 })
{
    var pager = new ReportViewModel { TongKetQua = 200, TrangHienTai = current };
    var visible = pager.TrangHienThi;
    Check(visible.First() == 1 && visible.Last() == 20 && visible.Contains(current), "Pager must expose first, last and current page");
    Check(visible.Count <= 7 && visible.Where(p => p.HasValue).Distinct().Count() == visible.Count(p => p.HasValue), "Pager must stay compact without repeating page numbers");
    Check(visible.Where(p => p.HasValue).All(p => p >= 1 && p <= 20), "Pager contains an invalid target");
}
Check(new ReportViewModel().TrangHienThi.SequenceEqual(new int?[] { 1 }), "Empty report must have one safe page");

if (args.Contains("--live"))
{
    // This mode only reads the configured development database; it never initializes it.
    using var live = Connect("QLThuVienUTC");
    var report = new ReportService(live).GetReport("Tất cả thời gian");
    Check(report.TongDauSach == live.TaiLieus.Count() && report.TongKetQua == live.TaiLieus.Count(), "Live title count differs from SQL");
    Check(report.TongBanDoc == live.BanDocs.Count(), "Live reader count differs from SQL");
    Check(report.TongBanSach == live.BanSaos.Count(), "Live copy count differs from SQL");
    Check(report.LuotMuonTrongThang == live.ChiTietMuons.Count(), "Live loan count differs from SQL");
    Check(report.LuotTraTrongThang == live.ChiTietMuons.Count(c => c.NgayTra != null), "Live return count differs from SQL");
    var today = DateTime.Today.ToString("yyyy-MM-dd");
    var overdue = live.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.ChiTietMuon WHERE NgayTra IS NULL AND TrangThai NOT IN (N'Đã trả', N'Đã đền tiền', N'Đã đền sách mới') AND (HanTra < {today} OR TrangThai = N'Quá hạn')").Single();
    Check(report.QuaHanHienTai == overdue, "Live overdue count differs from SQL");
    var rows = live.Database.SqlQuery<StockRow>($"SELECT t.MaTaiLieu, t.TenTaiLieu, COUNT(b.MaBanSao) AS Total, SUM(CASE WHEN b.TrangThai=N'Có sẵn' AND b.TinhTrang NOT IN (N'Mất', N'Hỏng') THEN 1 ELSE 0 END) AS Available, SUM(CASE WHEN b.TrangThai=N'Đang mượn' AND b.TinhTrang NOT IN (N'Mất', N'Hỏng') THEN 1 ELSE 0 END) AS Borrowed, SUM(CASE WHEN b.TrangThai IN (N'Đã mất', N'Mất', N'Hỏng', N'Đã hỏng') OR b.TinhTrang IN (N'Mất', N'Hỏng') THEN 1 ELSE 0 END) AS Damaged FROM dbo.TaiLieu t LEFT JOIN dbo.BanSao b ON b.MaTaiLieu=t.MaTaiLieu GROUP BY t.MaTaiLieu,t.TenTaiLieu ORDER BY t.TenTaiLieu,t.MaTaiLieu").ToList();
    var pagedRows = Enumerable.Range(1, report.TongTrang).SelectMany(p => new ReportService(live).GetReport("Tất cả thời gian", page: p).ChiTietDauSach).ToList();
    Check(rows.Select((r, i) => pagedRows[i] is var item && item.MaTaiLieu == r.MaTaiLieu && item.DauSach == r.TenTaiLieu &&
        item.TongBan == r.Total && item.SanCo == r.Available && item.DangMuon == r.Borrowed && item.MatHong == r.Damaged).All(x => x), "Live inventory rows differ from SQL");
    var current = new ReportService(live).GetReport(null);
    var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    var end = start.AddMonths(1);
    Check(current.LuotMuonTrongThang == live.ChiTietMuons.Count(c => c.MaPhieuMuonNavigation.NgayMuon >= start && c.MaPhieuMuonNavigation.NgayMuon < end), "Live monthly loans differ from SQL");
    Check(current.LuotTraTrongThang == live.ChiTietMuons.Count(c => c.NgayTra >= start && c.NgayTra < end), "Live monthly returns differ from SQL");
    foreach (var month in current.XuHuong)
    {
        var monthStart = DateTime.ParseExact(month.Thang, "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        var monthEnd = monthStart.AddMonths(1);
        Check(month.Muon == live.ChiTietMuons.Count(c => c.MaPhieuMuonNavigation.NgayMuon >= monthStart && c.MaPhieuMuonNavigation.NgayMuon < monthEnd), "Live chart loans differ from SQL");
        Check(month.Tra == live.ChiTietMuons.Count(c => c.NgayTra >= monthStart && c.NgayTra < monthEnd), "Live chart returns differ from SQL");
    }
    Console.WriteLine($"PASS live SQL: {report.TongDauSach} titles over {report.TongTrang} pages, {report.TongBanDoc} readers, {report.TongBanSach} copies, {report.TongCongSanCo} available, {report.TongCongDangMuon} borrowed, {report.TongCongMatHong} lost/damaged; {report.LuotMuonTrongThang} loans, {report.LuotTraTrongThang} returns, {report.QuaHanHienTai} overdue.");
    Console.WriteLine($"PASS current month: {current.KyBaoCao}, {current.LuotMuonTrongThang} loans, {current.LuotTraTrongThang} returns; every inventory row and all six chart months match raw SQL.");
    return;
}

var databaseName = "QLThuVienUTC_ReportCheck_" + Guid.NewGuid().ToString("N")[..8];
using var db = Connect(databaseName);
try
{
    db.Database.EnsureCreated();
    var clock = new ReportClock();
    var service = new ReportService(db, clock);
    var empty = service.GetReport(null);
    Check(empty.TongBanSach == 0 && empty.LuotMuonTrongThang == 0 && empty.QuaHanHienTai == 0 && empty.ChiTietDauSach.Count == 0, "Empty SQL must produce empty report");
    var staff = new NhanVien { HoTen = "Report Test", MaSoNhanVien = "REPORT", TrangThai = "Hoạt động" };
    var a = new TaiLieu { TenTaiLieu = "Đường dẫn <SQL> & sách", Isbn = "978TEST00001" };
    var b = new TaiLieu { TenTaiLieu = a.TenTaiLieu };
    var digital = new TaiLieu { TenTaiLieu = "Không có bản giấy" };
    BanSao Copy(TaiLieu title, string barcode, string state, string condition = "Tốt") => new()
    {
        MaTaiLieuNavigation = title, MaVach = barcode, TrangThai = state, TinhTrang = condition
    };
    var copies = new[]
    {
        Copy(a, "R1", "Có sẵn"), Copy(a, "R2", "Đang mượn"), Copy(a, "R3", "Đã mất", "Mất"), Copy(a, "R4", "Đang sửa"),
        Copy(b, "R5", "Đang mượn"), Copy(b, "R6", "Có sẵn", "Hỏng"), Copy(b, "R7", "Có sẵn")
    };
    db.BanSaos.AddRange(copies);
    db.TaiLieus.Add(digital);
    db.BanDocs.AddRange(new BanDoc { MaSinhVien = "REPORTSV", HoTen = "Report Student", TrangThai = "Hoạt động" },
        new BanDoc { MaSinhVien = "REPORTGV", HoTen = "Report Lecturer", TrangThai = "Hoạt động" });
    PhieuMuon Loan(DateTime borrowed, params ChiTietMuon[] details) => new()
    {
        MaNhanVienNavigation = staff, NgayMuon = borrowed, TrangThai = "Đang mượn", ChiTietMuons = details
    };
    ChiTietMuon Detail(int copy, DateOnly due, DateTime? returned = null, string state = "Đang mượn") => new()
    {
        MaBanSaoNavigation = copies[copy], HanTra = due, NgayTra = returned, TrangThai = state
    };
    var due = new DateOnly(2026, 12, 31);
    db.PhieuMuons.AddRange(
        Loan(new DateTime(2026, 10, 31, 23, 59, 59), Detail(0, due, new DateTime(2026, 11, 1), "Đã trả")),
        Loan(new DateTime(2026, 11, 1), Detail(1, new DateOnly(2026, 11, 10)), Detail(4, new DateOnly(2026, 11, 11))),
        Loan(new DateTime(2026, 11, 30, 23, 59, 59), Detail(6, due, new DateTime(2026, 12, 1), "Đã trả")),
        Loan(new DateTime(2026, 12, 1), Detail(0, due)),
        Loan(new DateTime(2024, 12, 1), Detail(2, new DateOnly(2024, 12, 31), state: "Đã đền tiền")),
        Loan(new DateTime(2026, 1, 2), Detail(5, new DateOnly(2026, 5, 31), new DateTime(2026, 10, 31, 23, 59, 59), "Đã trả")));
    db.SaveChanges();

    var month = service.GetReport(null);
    Check(month.KyBaoCao == "Tháng 11/2026" && month.LuotMuonTrongThang == 3 && month.LuotTraTrongThang == 1, "Month must count copies, include first/last day, and filter loans and returns by their own dates");
    Check(month.QuaHanHienTai == 1, "Overdue must exclude returned/compensated loans and include only deadlines before today");
    Check(month.TongBanSach == 7 && month.TongCongSanCo == 2 && month.TongCongDangMuon == 2 && month.TongCongMatHong == 2 && month.TongCongKhac == 1, "Inventory must count actual copies with disjoint states");
    Check(month.TongBanDoc == 2, "Reader total must count SQL reader records");
    Check(month.ChiTietDauSach.Count == 3 && month.ChiTietDauSach.Count(t => t.DauSach == a.TenTaiLieu) == 2 && month.ChiTietDauSach.Any(t => t.TongBan == 0), "Titles must join by ID, including duplicate names and zero-copy titles");
    Check(month.ChiTietDauSach.All(t => t.TongBan == t.SanCo + t.DangMuon + t.MatHong + t.Khac), "Row totals must reconcile");
    var quarter = service.GetReport("Quý 4/2026");
    Check(quarter.LuotMuonTrongThang == 5 && quarter.LuotTraTrongThang == 3, "Quarter dates are incorrect");
    var year = service.GetReport("Năm 2026");
    Check(year.LuotMuonTrongThang == 6 && year.LuotTraTrongThang == 3, "Year dates are incorrect");
    var all = service.GetReport("Tất cả thời gian");
    Check(all.LuotMuonTrongThang == 7 && all.LuotTraTrongThang == 3, "All-time counts are incorrect");
    Check(all.DanhSachKyBaoCao.Contains("Tháng 12/2024"), "Historic SQL activity must be selectable");
    var noActivity = service.GetReport("Tháng 09/2026");
    Check(noActivity.LuotMuonTrongThang == 0 && noActivity.LuotTraTrongThang == 0 && noActivity.TongBanSach == 7 && noActivity.QuaHanHienTai == 1, "Current inventory and overdue must not change with reporting period");
    Check(service.GetReport("invalid period").KyBaoCao == "Tháng 11/2026", "Invalid period should fall back to the current month");
    var oneDay = service.GetReport(null, startDate: new DateOnly(2026, 11, 1), endDate: new DateOnly(2026, 11, 1));
    Check(oneDay.LuotMuonTrongThang == 2 && oneDay.LuotTraTrongThang == 1, "Date range must include the full final day and count independent return dates");
    var range = service.GetReport(null, startDate: new DateOnly(2026, 10, 31), endDate: new DateOnly(2026, 11, 1));
    Check(range.LuotMuonTrongThang == 3 && range.LuotTraTrongThang == 2, "Custom range boundaries are wrong");
    Check(month.XuHuong.Count == 6 && month.XuHuong.Last().Thang == "11/2026" && month.XuHuong.Last().Muon == 3 && month.XuHuong.Last().Tra == 1, "Monthly chart must use SQL activity");
    var matching = service.GetReport(null, "duong dan");
    Check(matching.TongKetQua == 2 && matching.KhoDaLoc.TongBan == 7 && matching.TongBanSach == 7 && matching.TongBanDoc == 2, "Unaccented search must support đ and retain global totals");
    Check(service.GetReport(null, "978TEST00001").TongKetQua == 1 && service.GetReport(null, "978TEST00001").KhoDaLoc.TongBan == 4, "ISBN search must filter the inventory footer");
    Check(service.GetReport(null, "%").TongKetQua == 0, "Search wildcard characters must be treated literally");
    var excel = Encoding.UTF8.GetString(service.GenerateExcelReport("Tháng 11/2026"));
    Check(excel.Contains(WebUtility.HtmlEncode(a.TenTaiLieu)) && !excel.Contains("<SQL>") && excel.Contains("<th>Khác</th>"), "Export must encode titles and include all inventory states");
    Check(excel.Contains("Lượt mượn trong kỳ</td><td class='num'>3</td>") && excel.Contains("Lượt trả trong kỳ</td><td class='num'>1</td>"), "Export must use the selected SQL period");
    Check(!db.ChangeTracker.Entries().Any(e => e.State != EntityState.Unchanged), "Reporting must not modify entities");
    copies[3].TrangThai = "Có sẵn";
    db.SaveChanges();
    Check(!service.GetReport(null).CoBanKhac && service.GetReport(null).TongCongSanCo == 3, "Fresh SQL changes must appear without stale JSON/cache counts");
    db.TaiLieus.AddRange(Enumerable.Range(1, 23).Select(i => new TaiLieu { TenTaiLieu = $"Paged title {i:D2}" }));
    db.SaveChanges();
    var firstPage = service.GetReport(null);
    Check(firstPage.ChiTietDauSach.Count == 10 && firstPage.TongKetQua == 26 && firstPage.TongTrang == 3 && firstPage.TongBanSach == 7, "Inventory must page titles independently of global stock");
    var allIds = Enumerable.Range(1, firstPage.TongTrang).SelectMany(p => service.GetReport(null, page: p).ChiTietDauSach).Select(x => x.MaTaiLieu).ToList();
    Check(allIds.Count == 26 && allIds.Distinct().Count() == 26, "Pages must not skip or repeat titles");
    Check(service.GetReport(null, page: int.MaxValue).TrangHienTai == 3 && service.GetReport(null, page: -1).TrangHienTai == 1, "Out-of-range pages must be clamped safely");
    var noMatch = service.GetReport(null, "not-a-real-book");
    Check(noMatch.TongKetQua == 0 && noMatch.KhoDaLoc.TongBan == 0 && noMatch.TongBanSach == 7, "No-match search must preserve real global totals");
    var fullExport = Encoding.UTF8.GetString(service.GenerateExcelReport(null));
    Check(Enumerable.Range(1, 23).All(i => fullExport.Contains($"Paged title {i:D2}")), "Excel must export every matching title across all pages");
    var filteredExport = Encoding.UTF8.GetString(service.GenerateExcelReport(null, "978TEST00001", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1)));
    Check(filteredExport.Contains("Lượt mượn trong kỳ</td><td class='num'>2</td>") && !filteredExport.Contains("Paged title"), "Excel must retain date and search filters");
    var invalidController = new ReportController(service, NullLogger<ReportController>.Instance) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    invalidController.Request.Headers["X-Report-Partial"] = "inventory";
    var partialResult = (PartialViewResult)invalidController.Index(null, search: "duong dan", page: 2);
    Check(partialResult.ViewName == "~/Views/Shared/Admin/_ReportInventory.cshtml" && ((ReportViewModel)partialResult.Model!).TimKiem == "duong dan", "Async inventory must use the shared partial and retain the search");
    var badRange = (ViewResult)invalidController.Index(null, startDate: new DateOnly(2026, 12, 1), endDate: new DateOnly(2026, 11, 1));
    Check(invalidController.Response.StatusCode == 400 && ((ReportViewModel)badRange.Model!).LoiBoLoc != null, "Invalid date ranges must show a usable validation message");
    Check(invalidController.ExportExcel(null, startDate: new DateOnly(2026, 12, 1), endDate: new DateOnly(2026, 11, 1)) is ObjectResult { StatusCode: 400 }, "Invalid date ranges must not export a report");
    Console.WriteLine("PASS: SQL reader count, date filters, monthly charts, accent/ISBN search, matching totals, bounded pagination and complete filtered export.");
    Console.WriteLine("PASS: SQL inventory, duplicate titles, zero copies, lost/damaged/other states, current overdue, month/quarter/year/all-time boundaries, independent return dates, empty periods, fresh SQL changes and Excel parity.");
}
finally
{
    if (!databaseName.StartsWith("QLThuVienUTC_ReportCheck_")) throw new InvalidOperationException("Unsafe test database name");
    db.Database.EnsureDeleted();
    Console.WriteLine("Removed disposable report database.");
}

// Verify the missing-database state without dropping or recreating the project database.
using var missing = Connect("QLThuVienUTC_ReportMissing_" + Guid.NewGuid().ToString("N")[..8]);
var controller = new ReportController(new ReportService(missing), NullLogger<ReportController>.Instance)
{
    ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
};
var failed = (ViewResult)controller.Index(null);
Check(controller.Response.StatusCode == 503 && ((ReportViewModel)failed.Model!).LoiDuLieu != null, "Missing SQL must display an error instead of fabricated zero counts");
Check(controller.ExportExcel(null) is ObjectResult { StatusCode: 503 }, "Missing SQL must not export a fake report");
Console.WriteLine("PASS: missing database returns 503 on page/export without creating a database.");

sealed class ReportClock : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(2026, 11, 11, 5, 0, 0, TimeSpan.Zero);
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.CreateCustomTimeZone("ReportUTC7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
}

sealed class StockRow
{
    public int MaTaiLieu { get; set; }
    public string TenTaiLieu { get; set; } = "";
    public int Total { get; set; }
    public int Available { get; set; }
    public int Borrowed { get; set; }
    public int Damaged { get; set; }
}
