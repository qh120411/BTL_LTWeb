using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Services;

public class ReportService : IReportService
{
    private readonly QLThuVienUtcContext _db;
    private readonly TimeProvider _clock;

    public ReportService(QLThuVienUtcContext db, TimeProvider? clock = null)
    {
        _db = db;
        _clock = clock ?? TimeProvider.System;
    }

    public ReportViewModel GetReport(string? period, string? search = null, int page = 1, DateOnly? startDate = null, DateOnly? endDate = null)
        => BuildReport(period, search, page, startDate, endDate, exportAll: false);

    private ReportViewModel BuildReport(string? period, string? search, int page, DateOnly? startDate, DateOnly? endDate, bool exportAll)
    {
        var now = _clock.GetLocalNow().DateTime;
        var today = DateOnly.FromDateTime(now);
        // Keep totals and detail rows consistent during concurrent borrowing and returns.
        using var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable);
        var loans = _db.ChiTietMuons.AsNoTracking();
        var activityMonths = loans
            .Select(c => new { Year = c.MaPhieuMuonNavigation.NgayMuon.Year, Month = c.MaPhieuMuonNavigation.NgayMuon.Month })
            .Union(loans.Where(c => c.NgayTra.HasValue)
                .Select(c => new { Year = c.NgayTra!.Value.Year, Month = c.NgayTra.Value.Month }))
            .ToList();
        var months = activityMonths.Select(m => new DateTime(m.Year, m.Month, 1))
            .Concat(Enumerable.Range(0, 12).Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(-i)))
            .Distinct().OrderByDescending(d => d).ToList();
        var periods = new List<(string Label, DateTime? Start, DateTime? End)>
        {
            ("Tất cả thời gian", null, null)
        };
        periods.AddRange(months.Select(m => ($"Tháng {m.ToString("MM/yyyy", CultureInfo.InvariantCulture)}", (DateTime?)m, (DateTime?)m.AddMonths(1))));
        periods.AddRange(months.Select(m => new DateTime(m.Year, (m.Month - 1) / 3 * 3 + 1, 1))
            .Distinct().Select(q => ($"Quý {(q.Month - 1) / 3 + 1}/{q.Year}", (DateTime?)q, (DateTime?)q.AddMonths(3))));
        periods.AddRange(months.Select(m => m.Year).Distinct()
            .Select(y => ($"Năm {y}", (DateTime?)new DateTime(y, 1, 1), (DateTime?)new DateTime(y + 1, 1, 1))));
        var selected = periods.FirstOrDefault(p => p.Label == period?.Trim());
        if (selected.Label == null)
            selected = periods.Single(p => p.Start == new DateTime(now.Year, now.Month, 1) && p.Label.StartsWith("Tháng "));

        if (startDate.HasValue || endDate.HasValue)
        {
            if (!startDate.HasValue || !endDate.HasValue || startDate > endDate)
                throw new ArgumentException("Chọn đủ hai ngày và để ngày bắt đầu trước hoặc bằng ngày kết thúc.");
            selected = ($"{startDate:dd/MM/yyyy} – {endDate:dd/MM/yyyy}", startDate.Value.ToDateTime(TimeOnly.MinValue),
                endDate == DateOnly.MaxValue ? null : endDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
        }

        var borrowed = loans;
        var returned = loans.Where(c => c.NgayTra.HasValue);
        if (selected.Start is DateTime start)
        {
            borrowed = borrowed.Where(c => c.MaPhieuMuonNavigation.NgayMuon >= start);
            returned = returned.Where(c => c.NgayTra >= start);
        }
        if (selected.End is DateTime end)
        {
            borrowed = borrowed.Where(c => c.MaPhieuMuonNavigation.NgayMuon < end);
            returned = returned.Where(c => c.NgayTra < end);
        }

        var inventory = _db.TaiLieus.AsNoTracking().Select(t => new BookReportItemViewModel
        {
            MaTaiLieu = t.MaTaiLieu,
            DauSach = t.TenTaiLieu,
            TongBan = t.BanSaos.Count,
            SanCo = t.BanSaos.Count(b => b.TrangThai == "Có sẵn" && b.TinhTrang != "Mất" && b.TinhTrang != "Hỏng"),
            DangMuon = t.BanSaos.Count(b => b.TrangThai == "Đang mượn" && b.TinhTrang != "Mất" && b.TinhTrang != "Hỏng"),
            MatHong = t.BanSaos.Count(b => b.TrangThai == "Đã mất" || b.TrangThai == "Mất" ||
                b.TrangThai == "Hỏng" || b.TrangThai == "Đã hỏng" || b.TinhTrang == "Mất" || b.TinhTrang == "Hỏng")
        });
        static InventoryTotalsViewModel Totals(IQueryable<BanSao> query) => query.GroupBy(_ => 1)
            .Select(g => new InventoryTotalsViewModel
            {
                TongBan = g.Count(),
                SanCo = g.Count(b => b.TrangThai == "Có sẵn" && b.TinhTrang != "Mất" && b.TinhTrang != "Hỏng"),
                DangMuon = g.Count(b => b.TrangThai == "Đang mượn" && b.TinhTrang != "Mất" && b.TinhTrang != "Hỏng"),
                MatHong = g.Count(b => b.TrangThai == "Đã mất" || b.TrangThai == "Mất" ||
                    b.TrangThai == "Hỏng" || b.TrangThai == "Đã hỏng" || b.TinhTrang == "Mất" || b.TinhTrang == "Hỏng")
            }).SingleOrDefault() ?? new();

        var stock = Totals(_db.BanSaos.AsNoTracking());
        search = (search ?? string.Empty).Trim();
        if (search.Length > 100) search = search[..100];
        var documents = _db.TaiLieus.AsNoTracking();
        if (search.Length > 0)
        {
            var keyword = search.Replace('đ', 'd').Replace('Đ', 'D');
            documents = documents.Where(t => EF.Functions.Collate(t.TenTaiLieu.Replace("đ", "d").Replace("Đ", "D"), "Latin1_General_100_CI_AI").Contains(keyword) ||
                (t.Isbn != null && t.Isbn.Contains(search)));
        }
        // Filter by document ID so duplicate titles remain separate inventory records.
        var ids = documents.Select(t => t.MaTaiLieu);
        var filteredInventory = inventory.Where(t => ids.Contains(t.MaTaiLieu)).OrderBy(t => t.DauSach).ThenBy(t => t.MaTaiLieu);
        var totalTitles = documents.Count();
        const int pageSize = 10;
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalTitles / (double)pageSize)));

        var trendEnd = endDate?.ToDateTime(TimeOnly.MinValue) ?? selected.End?.AddTicks(-1) ?? now;
        var trendLastMonth = new DateTime(trendEnd.Year, trendEnd.Month, 1);
        if (trendLastMonth < new DateTime(1, 6, 1)) trendLastMonth = new DateTime(1, 6, 1);
        var trendFirstMonth = trendLastMonth.AddMonths(-5);
        var trendExclusiveEnd = trendLastMonth == new DateTime(9999, 12, 1) ? DateTime.MaxValue : trendLastMonth.AddMonths(1);
        var monthlyLoans = loans.Where(c => c.MaPhieuMuonNavigation.NgayMuon >= trendFirstMonth && c.MaPhieuMuonNavigation.NgayMuon < trendExclusiveEnd)
            .GroupBy(c => new { c.MaPhieuMuonNavigation.NgayMuon.Year, c.MaPhieuMuonNavigation.NgayMuon.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToList();
        var monthlyReturns = loans.Where(c => c.NgayTra >= trendFirstMonth && c.NgayTra < trendExclusiveEnd)
            .GroupBy(c => new { c.NgayTra!.Value.Year, c.NgayTra.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToList();

        var model = new ReportViewModel
        {
            KyBaoCao = selected.Label,
            DanhSachKyBaoCao = periods.Select(p => p.Label).ToList(),
            TuNgay = selected.Start.HasValue ? DateOnly.FromDateTime(selected.Start.Value) : null,
            DenNgay = endDate ?? (selected.End.HasValue ? DateOnly.FromDateTime(selected.End.Value.AddDays(-1)) : null),
            TimKiem = search,
            TongBanDoc = _db.BanDocs.Count(),
            TongDauSach = _db.TaiLieus.Count(),
            TongKho = stock,
            KhoDaLoc = search.Length == 0 ? stock : Totals(_db.BanSaos.AsNoTracking().Where(b => ids.Contains(b.MaTaiLieu))),
            TongKetQua = totalTitles,
            TrangHienTai = page,
            KichThuocTrang = pageSize,
            XuHuong = Enumerable.Range(0, 6).Select(i => trendFirstMonth.AddMonths(i)).Select(m => new CirculationTrendItem
            {
                Thang = m.ToString("MM/yyyy", CultureInfo.InvariantCulture),
                Muon = monthlyLoans.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Count ?? 0,
                Tra = monthlyReturns.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Count ?? 0
            }).ToList(),
            LuotMuonTrongThang = borrowed.Count(),
            LuotTraTrongThang = returned.Count(),
            QuaHanHienTai = loans.Count(c => c.NgayTra == null && c.TrangThai != "Đã trả" &&
                c.TrangThai != "Đã đền tiền" && c.TrangThai != "Đã đền sách mới" &&
                (c.HanTra < today || c.TrangThai == "Quá hạn")),
            ChiTietDauSach = exportAll ? filteredInventory.ToList() : filteredInventory.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };
        transaction.Commit();
        return model;
    }

    public byte[] GenerateExcelReport(string? period, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var report = BuildReport(period, search, 1, startDate, endDate, exportAll: true);
        var columns = report.CoBanKhac ? 6 : 5;
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>table{border-collapse:collapse;font-family:Arial;font-size:13px}th,td{border:1px solid #ccc;padding:8px}th{background:#eef4f8}.num{text-align:right}</style></head><body><table>");
        sb.AppendLine($"<tr><th colspan='{columns}'>BÁO CÁO THƯ VIỆN</th></tr>");
        sb.AppendLine($"<tr><td colspan='{columns}'>Kỳ báo cáo: {WebUtility.HtmlEncode(report.KyBaoCao)} | Toàn thư viện</td></tr>");
        sb.AppendLine($"<tr><td colspan='{columns}'>Ngày xuất: {_clock.GetLocalNow():dd/MM/yyyy HH:mm}</td></tr>");
        foreach (var (label, count) in new[]
        {
            ("Lượt mượn trong kỳ", report.LuotMuonTrongThang),
            ("Lượt trả trong kỳ", report.LuotTraTrongThang),
            ("Đang quá hạn", report.QuaHanHienTai),
            ("Tổng bản sách hiện tại", report.TongBanSach),
            ("Tổng bạn đọc", report.TongBanDoc)
        })
            sb.AppendLine($"<tr><td colspan='{columns - 1}'>{label}</td><td class='num'>{count}</td></tr>");
        sb.AppendLine($"<tr><th colspan='{columns}'>Tồn kho hiện tại</th></tr>");
        if (report.TimKiem.Length > 0)
            sb.AppendLine($"<tr><td colspan='{columns}'>Tìm sách: {WebUtility.HtmlEncode(report.TimKiem)}</td></tr>");
        sb.AppendLine("<tr><th>Tên sách</th><th>Tổng bản</th><th>Sẵn có</th><th>Đang mượn</th><th>Mất / hỏng</th>" + (report.CoBanKhac ? "<th>Khác</th>" : "") + "</tr>");
        foreach (var item in report.ChiTietDauSach)
            AppendRow(item.DauSach, item.TongBan, item.SanCo, item.DangMuon, item.MatHong, item.Khac);
        AppendRow($"Tổng {report.TongKetQua} đầu sách", report.KhoDaLoc.TongBan, report.KhoDaLoc.SanCo, report.KhoDaLoc.DangMuon, report.KhoDaLoc.MatHong, report.KhoDaLoc.Khac);
        sb.AppendLine("</table></body></html>");
        return Encoding.UTF8.GetBytes(sb.ToString());

        void AppendRow(string title, int total, int available, int borrowed, int damaged, int other)
        {
            sb.Append($"<tr><td>{WebUtility.HtmlEncode(title)}</td><td class='num'>{total}</td><td class='num'>{available}</td><td class='num'>{borrowed}</td><td class='num'>{damaged}</td>");
            if (report.CoBanKhac) sb.Append($"<td class='num'>{other}</td>");
            sb.AppendLine("</tr>");
        }
    }
}
