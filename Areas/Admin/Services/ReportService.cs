using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Services;

public class ReportService : IReportService
{
    public ReportViewModel GetReport(string? period, string? scope)
    {
        var model = new ReportViewModel
        {
            KyBaoCao = string.IsNullOrWhiteSpace(period) ? "Tháng 10/2026" : period,
            PhamVi = string.IsNullOrWhiteSpace(scope) ? "Toàn thư viện" : scope
        };

        if (model.KyBaoCao == "Tháng 10/2026")
        {
            model.LuotMuonTrongThang = 42;
            model.LuotTraTrongThang = 37;
            model.QuaHanHienTai = 1;
            model.TongBanSach = 65;

            model.ChiTietDauSach = new List<BookReportItemViewModel>
            {
                new() { DauSach = "Lập trình Python cơ bản", TongBan = 20, SanCo = 11, DangMuon = 8, MatHong = 1 },
                new() { DauSach = "Cơ sở dữ liệu", TongBan = 15, SanCo = 8, DangMuon = 7, MatHong = 0 },
                new() { DauSach = "Kỹ thuật điện", TongBan = 12, SanCo = 9, DangMuon = 3, MatHong = 0 },
                new() { DauSach = "Quản trị logistics", TongBan = 18, SanCo = 10, DangMuon = 8, MatHong = 0 }
            };
        }
        else
        {
            // Các kỳ báo cáo khác để trải nghiệm sinh động khi người dùng chọn lọc
            model.LuotMuonTrongThang = 35;
            model.LuotTraTrongThang = 32;
            model.QuaHanHienTai = 2;
            model.TongBanSach = 65;

            model.ChiTietDauSach = new List<BookReportItemViewModel>
            {
                new() { DauSach = "Lập trình Python cơ bản", TongBan = 20, SanCo = 13, DangMuon = 6, MatHong = 1 },
                new() { DauSach = "Cơ sở dữ liệu", TongBan = 15, SanCo = 10, DangMuon = 5, MatHong = 0 },
                new() { DauSach = "Kỹ thuật điện", TongBan = 12, SanCo = 10, DangMuon = 2, MatHong = 0 },
                new() { DauSach = "Quản trị logistics", TongBan = 18, SanCo = 11, DangMuon = 7, MatHong = 0 }
            };
        }

        return model;
    }

    public byte[] GenerateExcelReport(string? period, string? scope)
    {
        var report = GetReport(period, scope);

        // Tạo file HTML-based Excel Table định dạng chuẩn hiển thị trực tiếp trong Microsoft Excel / Google Sheets
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
        sb.AppendLine("<!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>Báo cáo lưu thông</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->");
        sb.AppendLine("<style>");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; font-family: Segoe UI, Tahoma, sans-serif; font-size: 13px; }");
        sb.AppendLine("th, td { border: 1px solid #c2c2c2; padding: 8px 12px; }");
        sb.AppendLine("th { background-color: #0072bc; color: #ffffff; text-align: center; font-weight: bold; }");
        sb.AppendLine(".title { font-size: 18px; font-weight: bold; color: #0072bc; text-align: center; }");
        sb.AppendLine(".meta { font-size: 13px; color: #555555; }");
        sb.AppendLine(".kpi-table { margin-bottom: 20px; }");
        sb.AppendLine(".kpi-header { background-color: #eaf3f8; font-weight: bold; }");
        sb.AppendLine(".num { text-align: right; }");
        sb.AppendLine(".total-row { font-weight: bold; background-color: #f2f7fb; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<table>");
        sb.AppendLine($"<tr><td colspan='5' class='title'>THƯ VIỆN ĐẠI HỌC GIAO THÔNG VẬN TẢI</td></tr>");
        sb.AppendLine($"<tr><td colspan='5' class='title'>BÁO CÁO & THỐNG KÊ LƯU THÔNG HỌC LIỆU</td></tr>");
        sb.AppendLine($"<tr><td colspan='5' class='meta'>Kỳ báo cáo: {report.KyBaoCao} | Phạm vi: {report.PhamVi} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>");
        sb.AppendLine("<tr><td colspan='5'></td></tr>");

        // KPI
        sb.AppendLine("<tr><th colspan='5'>TỔNG QUAN CHỈ SỐ</th></tr>");
        sb.AppendLine($"<tr><td colspan='2'>Lượt mượn trong tháng:</td><td colspan='3' class='num'><b>{report.LuotMuonTrongThang}</b></td></tr>");
        sb.AppendLine($"<tr><td colspan='2'>Lượt trả trong tháng:</td><td colspan='3' class='num'><b>{report.LuotTraTrongThang}</b></td></tr>");
        sb.AppendLine($"<tr><td colspan='2'>Quá hạn hiện tại:</td><td colspan='3' class='num'><b>{report.QuaHanHienTai}</b></td></tr>");
        sb.AppendLine($"<tr><td colspan='2'>Tổng bản sách:</td><td colspan='3' class='num'><b>{report.TongBanSach}</b></td></tr>");
        sb.AppendLine("<tr><td colspan='5'></td></tr>");

        // Data Table
        sb.AppendLine("<tr>");
        sb.AppendLine("<th style='text-align: left;'>Đầu sách</th>");
        sb.AppendLine("<th>Tổng bản</th>");
        sb.AppendLine("<th>Sẵn có</th>");
        sb.AppendLine("<th>Đang mượn</th>");
        sb.AppendLine("<th>Mất / hỏng</th>");
        sb.AppendLine("</tr>");

        foreach (var item in report.ChiTietDauSach)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style='text-align: left;'>{item.DauSach}</td>");
            sb.AppendLine($"<td class='num'>{item.TongBan}</td>");
            sb.AppendLine($"<td class='num'>{item.SanCo}</td>");
            sb.AppendLine($"<td class='num'>{item.DangMuon}</td>");
            sb.AppendLine($"<td class='num'>{item.MatHong}</td>");
            sb.AppendLine("</tr>");
        }

        // Total Row
        sb.AppendLine("<tr class='total-row'>");
        sb.AppendLine("<td>TỔNG</td>");
        sb.AppendLine($"<td class='num'>{report.TongCongSoBan}</td>");
        sb.AppendLine($"<td class='num'>{report.TongCongSanCo}</td>");
        sb.AppendLine($"<td class='num'>{report.TongCongDangMuon}</td>");
        sb.AppendLine($"<td class='num'>{report.TongCongMatHong}</td>");
        sb.AppendLine("</tr>");

        sb.AppendLine("</table>");
        sb.AppendLine("<p><i>* Ghi chú: Báo cáo gồm lượt mượn / trả theo tháng, đầu sách và trạng thái từng bản sách. Số liệu minh họa UTC Library.</i></p>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }
}

