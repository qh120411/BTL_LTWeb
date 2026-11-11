using System.Security.Claims;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Controllers
{
    public class ApiDocController : Controller
    {
        private readonly QLThuVienUtcContext _dbContext;
        private readonly IBookCatalogService _catalogService;
        private readonly IReaderService _readerService;
        private readonly IUserProfileService _userProfileService;
        private readonly TimeProvider _timeProvider;

        public ApiDocController(
            QLThuVienUtcContext dbContext,
            IBookCatalogService catalogService,
            IReaderService readerService,
            IUserProfileService userProfileService,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _catalogService = catalogService;
            _readerService = readerService;
            _userProfileService = userProfileService;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        [HttpGet("/ApiDoc")]
        [HttpGet("/api-docs")]
        [HttpGet("/swagger")]
        [HttpGet("/Docs/Api")]
        public IActionResult Index() => View();

        // 1. Kiểm tra trạng thái máy chủ & CSDL
        [HttpGet("/api/system/health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            bool dbConnected = false;
            try { dbConnected = await _dbContext.Database.CanConnectAsync(); } catch { }

            return Ok(new
            {
                status = "Healthy",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                databaseConnected = dbConnected,
                totalBooks = _catalogService.GetAll().Count(),
                totalReaders = _readerService.GetReaders(null, null).Readers.Count,
                activeLoans = _readerService.GetAllCirculationTransactions().Count
            });
        }

        // 2. Phiên đăng nhập hiện tại
        [HttpGet("/api/auth/profile")]
        public IActionResult GetCurrentUserProfile()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Ok(new { isAuthenticated = false, user = (object?)null });

            return Ok(new
            {
                isAuthenticated = true,
                username = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity.Name,
                fullName = User.FindFirstValue(ClaimTypes.Name),
                roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Distinct()
            });
        }

        // 3. Danh mục học liệu (tìm kiếm & phân trang)
        [HttpGet("/api/catalog/books")]
        public IActionResult GetBooks(string? keyword, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var list = string.IsNullOrWhiteSpace(keyword)
                ? _catalogService.GetAll().ToList()
                : _catalogService.Search(keyword, null, null, null, null).ToList();

            var paged = list.Skip((int)Math.Min((long)(page - 1) * pageSize, int.MaxValue)).Take(pageSize).Select(b => new
            {
                b.Id,
                b.Title,
                author = b.Authors,
                b.Major,
                b.Year,
                b.Shelf,
                b.AvailableCopies,
                b.TotalCopies,
                status = b.AvailableCopies > 0 ? "Có sẵn" : "Hết sách"
            });

            return Ok(new { total = list.Count, page, pageSize, data = paged });
        }

        // 4. Chi tiết sách theo ID
        [HttpGet("/api/catalog/books/{id:int}")]
        public IActionResult GetBookById(int id)
        {
            var book = _catalogService.GetById(id);
            if (book == null) return NotFound(new { message = $"Không tìm thấy sách ID: {id}" });

            return Ok(new { book, copies = _catalogService.GetCopiesByBookId(id) });
        }

        // 5. Danh sách bản sao vật lý của sách
        [HttpGet("/api/catalog/books/{id:int}/copies")]
        public IActionResult GetBookCopies(int id)
        {
            return Ok(new { bookId = id, copies = _catalogService.GetCopiesByBookId(id) });
        }

        // 6. Danh sách phiếu mượn trả
        [HttpGet("/api/circulation/transactions")]
        [BTL_LTWEB.Common.ApiStaffAuthorize]
        public IActionResult GetCirculationTransactions(string? maBD)
        {
            var all = _readerService.GetAllCirculationTransactions();
            if (!string.IsNullOrWhiteSpace(maBD))
                all = all.Where(t => t.MaBD.Equals(maBD.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(new { total = all.Count, data = all });
        }

        public class BorrowReq { public string MaBD { get; set; } = string.Empty; public int BookId { get; set; } }

        // 7. Mượn sách: dùng chung nghiệp vụ SQL với giao diện Admin.
        [HttpPost("/api/circulation/borrow")]
        [BTL_LTWEB.Common.ApiStaffAuthorize]
        [ValidateAntiForgeryToken]
        public IActionResult BorrowBook([FromBody] BorrowReq req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.MaBD) || req.BookId <= 0)
                return BadRequest(new { message = "Vui lòng nhập maBD và bookId." });

            var maBD = req.MaBD.Trim();
            var borrowedAt = _timeProvider.GetLocalNow().DateTime;
            if (!BTL_LTWEB.Common.LoanTermPolicy.TryGetDueDate(DateOnly.FromDateTime(borrowedAt), out var dueDate))
                return BadRequest(new { success = false, message = BTL_LTWEB.Common.LoanTermPolicy.BorrowingWindowMessage });

            // Thử mượn qua SQL Server
            if (_readerService.BorrowBook(maBD, req.BookId, out string title, out string err, User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return Ok(new { success = true, mode = "SQL_Server", message = $"Mượn thành công '{title}' cho {maBD}" });

            return BadRequest(new { success = false, message = err });
        }

        public class ReturnReq { public string MaBD { get; set; } = string.Empty; public string? MaGD { get; set; } }

        // 8. Trả đúng giao dịch được chọn, không tự chọn sách khác.
        [HttpPost("/api/circulation/return")]
        [BTL_LTWEB.Common.ApiStaffAuthorize]
        [ValidateAntiForgeryToken]
        public IActionResult ReturnBook([FromBody] ReturnReq req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.MaBD))
                return BadRequest(new { message = "Vui lòng nhập maBD." });

            var maBD = req.MaBD.Trim();
            var maGD = req.MaGD?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(maGD))
                return BadRequest(new { success = false, message = "Vui lòng chọn đúng mã giao dịch cần trả." });
            if (_readerService.ReturnBook(maBD, maGD, out string bookTitle, out _, User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return Ok(new { success = true, mode = "SQL_Server", message = $"Đã nhận trả sách '{bookTitle}'" });
            return BadRequest(new { success = false, message = "Không tìm thấy giao dịch đang mượn của bạn đọc này hoặc chưa lưu được giao dịch." });
        }

        // 9. Danh sách bạn đọc
        [HttpGet("/api/readers")]
        [BTL_LTWEB.Common.ApiStaffAuthorize]
        public IActionResult GetReaders(string? keyword, string? group)
        {
            var result = _readerService.GetReaders(keyword, group);
            return Ok(new { total = result.Readers.Count, data = result.Readers });
        }

        // 10. Chi tiết bạn đọc theo Mã sinh viên / Mã thẻ
        [HttpGet("/api/readers/{id}")]
        [BTL_LTWEB.Common.ApiStaffAuthorize]
        public IActionResult GetReaderDetail(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { message = "Vui lòng nhập ID bạn đọc." });
            var cleanId = id.Trim();

            // 1. Thử từ ReaderService (SQL Server)
            var reader = _readerService.GetReaderById(cleanId);
            if (reader != null) return Ok(new { mode = "SQL_Server", data = reader });

            // 2. Thử theo MaBanDoc nếu id là số
            if (int.TryParse(cleanId, out int maBDInt))
            {
                var bd = _dbContext.BanDocs.FirstOrDefault(b => b.MaBanDoc == maBDInt);
                if (bd != null && (reader = _readerService.GetReaderById(bd.MaSinhVien)) != null)
                    return Ok(new { mode = "SQL_Server", data = reader });
            }

            return NotFound(new { message = $"Không tìm thấy bạn đọc '{cleanId}'" });
        }

        // 11. Thống kê tổng hợp Dashboard
        [HttpGet("/api/dashboard/stats")]
        [BTL_LTWEB.Common.ApiStaffAuthorize]
        public IActionResult GetDashboardStats() => Ok(_readerService.GetDashboardStatistics());
    }
}
