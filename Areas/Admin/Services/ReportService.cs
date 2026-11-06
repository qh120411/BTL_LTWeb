using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Services;
public class ReportService(IBookCatalogService catalog, QLThuVienUtcContext db) : IReportService
{
    public ReportViewModel GetReport(string? period, string? scope)
    {
        var books = catalog.GetAll().ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var loans = db.ChiTietMuons.AsNoTracking().Include(c => c.MaPhieuMuonNavigation).ToList();
        return new ReportViewModel
        {
            KyBaoCao = $"Tồn kho lúc {DateTime.Now:dd/MM/yyyy HH:mm}",
            PhamVi = "Toàn thư viện",
            TongBanSach = books.Sum(b => b.TotalCopies),
            LuotMuonTrongThang = loans.Count(c => c.MaPhieuMuonNavigation.NgayMuon.Year == DateTime.Today.Year && c.MaPhieuMuonNavigation.NgayMuon.Month == DateTime.Today.Month),
            LuotTraTrongThang = loans.Count(c => c.NgayTra?.Year == DateTime.Today.Year && c.NgayTra?.Month == DateTime.Today.Month),
            QuaHanHienTai = loans.Count(c => c.NgayTra == null && c.HanTra < today),
            ChiTietDauSach = books.Select(b => new BookReportItemViewModel { DauSach = b.Title, TongBan = b.TotalCopies, SanCo = b.AvailableCopies, DangMuon = b.Copies.Count(c => c.Status == "Đang mượn"), MatHong = b.Copies.Count(c => c.Status == "Đã mất" || c.Status == "Mất" || c.Status == "Hỏng") }).ToList()
        };
    }

    public byte[] GenerateExcelReport(string? period, string? scope)
    {
        var report = GetReport(period, scope);
        string Cell(string s)
        {
            if (s.Length > 0 && "=+-@".Contains(s[0]))
                s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        var rows = new List<string>
        {
            "Đầu sách,Tổng bản,Sẵn có,Đang mượn,Mất / hỏng"
        };
        rows.AddRange(report.ChiTietDauSach.Select(b => $"{Cell(b.DauSach)},{b.TongBan},{b.SanCo},{b.DangMuon},{b.MatHong}"));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(string.Join("\r\n", rows))).ToArray();
    }
}
