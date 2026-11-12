using System.Security.Claims;
using BTL_LTWEB.Areas.Admin.Services;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Controllers
{
    public class ApiDocController : Controller
    {
        private readonly QLThuVienUtcContext _dbContext;
        private readonly IBookCatalogService _catalogService;
        private readonly IReaderService _readerService;
        private readonly IUserProfileService _userProfileService;

        public ApiDocController(
            QLThuVienUtcContext dbContext,
            IBookCatalogService catalogService,
            IReaderService readerService,
            IUserProfileService userProfileService)
        {
            _dbContext = dbContext;
            _catalogService = catalogService;
            _readerService = readerService;
            _userProfileService = userProfileService;
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
            var list = string.IsNullOrWhiteSpace(keyword)
                ? _catalogService.GetAll().ToList()
                : _catalogService.Search(keyword, null, null, null, null).ToList();

            var paged = list.Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).Select(b => new
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
        public IActionResult GetCirculationTransactions(string? maBD)
        {
            var all = _readerService.GetAllCirculationTransactions();
            if (!string.IsNullOrWhiteSpace(maBD))
                all = all.Where(t => t.MaBD.Equals(maBD.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(new { total = all.Count, data = all });
        }

        public class BorrowReq { public string MaBD { get; set; } = string.Empty; public int BookId { get; set; } }

        // 7. MƯỢN SÁCH (POST) — Có cơ chế tự động Fallback khi DB lỗi
        [HttpPost("/api/circulation/borrow")]
        public IActionResult BorrowBook([FromBody] BorrowReq req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.MaBD) || req.BookId <= 0)
                return BadRequest(new { message = "Vui lòng nhập maBD và bookId." });

            var maBD = req.MaBD.Trim();

            // Thử mượn qua SQL Server
            if (_readerService.BorrowBook(maBD, req.BookId, out string title, out string err))
                return Ok(new { success = true, mode = "SQL_Server", message = $"Mượn thành công '{title}' cho {maBD}" });

            if (err.Contains("đạt giới hạn") || err.Contains("đã mượn đầu sách"))
                return BadRequest(new { success = false, message = err });

            // Fallback khi SQL Server offline / bạn đọc chưa có trong SQL
            var book = _catalogService.GetById(req.BookId);
            if (book == null) return NotFound(new { message = $"Không tìm thấy sách ID: {req.BookId}" });

            var profile = _userProfileService.GetProfile(maBD);
            if (profile.BorrowedBooks.Any(b => b.TenSach.Equals(book.Title, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { success = false, message = "Bạn đọc đã mượn đầu sách này rồi!" });

            string maGD = $"GD{DateTime.Now:yyMMdd}-{(profile.BorrowedBooks.Count + 1):D3}";
            profile.BorrowedBooks.Add(new BorrowedBookItem
            {
                MaGD = maGD,
                MaVach = $"UTC-{book.Id:D2}-01",
                TenSach = book.Title,
                TacGia = book.Authors,
                NgayMuon = DateTime.Now,
                HanTra = DateTime.Now.AddYears(1),
                TrangThaiText = "Đang mượn (Còn hạn)"
            });
            _userProfileService.UpdateProfile(profile);

            book.AvailableCopies = Math.Max(0, book.AvailableCopies - 1);
            _catalogService.UpdateBook(book, out _);

            return Ok(new { success = true, mode = "Offline_Fallback", message = $"Mượn thành công '{book.Title}' (Mã GD: {maGD})" });
        }

        public class ReturnReq { public string MaBD { get; set; } = string.Empty; public string? MaGD { get; set; } }

        // 8. TRẢ SÁCH (POST) — Tự động nhận diện sách đang mượn & Fallback
        [HttpPost("/api/circulation/return")]
        public IActionResult ReturnBook([FromBody] ReturnReq req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.MaBD))
                return BadRequest(new { message = "Vui lòng nhập maBD." });

            var maBD = req.MaBD.Trim();
            var maGD = req.MaGD?.Trim() ?? string.Empty;

            // Nếu không truyền maGD, tự động lấy giao dịch đang mượn đầu tiên
            if (string.IsNullOrEmpty(maGD))
            {
                var activeTx = _readerService.GetAllCirculationTransactions().FirstOrDefault(t => t.MaBD.Equals(maBD, StringComparison.OrdinalIgnoreCase));
                maGD = activeTx?.MaGD ?? _userProfileService.GetProfile(maBD).BorrowedBooks?.FirstOrDefault()?.MaGD ?? string.Empty;
            }

            // Thử trả qua SQL Server
            if (!string.IsNullOrEmpty(maGD) && _readerService.ReturnBook(maBD, maGD, out string bookTitle, out _))
                return Ok(new { success = true, mode = "SQL_Server", message = $"Đã nhận trả thành công sách '{bookTitle}'" });

            // Fallback trả qua Profile Storage khi DB offline
            var profile = _userProfileService.GetProfile(maBD);
            if (profile.BorrowedBooks == null || profile.BorrowedBooks.Count == 0)
                return BadRequest(new { success = false, message = $"Bạn đọc '{maBD}' hiện không có cuốn sách nào đang mượn để trả." });

            var target = profile.BorrowedBooks.FirstOrDefault(b => string.IsNullOrEmpty(maGD) || b.MaGD.Equals(maGD, StringComparison.OrdinalIgnoreCase)) ?? profile.BorrowedBooks.First();
            profile.BorrowedBooks.Remove(target);
            _userProfileService.UpdateProfile(profile);

            var catBook = _catalogService.GetAll().FirstOrDefault(b => b.Title.Equals(target.TenSach, StringComparison.OrdinalIgnoreCase));
            if (catBook != null) { catBook.AvailableCopies = Math.Min(catBook.TotalCopies, catBook.AvailableCopies + 1); _catalogService.UpdateBook(catBook, out _); }

            return Ok(new { success = true, mode = "Offline_Fallback", message = $"Đã nhận trả thành công sách '{target.TenSach}' (Mã GD: {target.MaGD})" });
        }

        // 9. Danh sách bạn đọc
        [HttpGet("/api/readers")]
        public IActionResult GetReaders(string? keyword, string? group)
        {
            var result = _readerService.GetReaders(keyword, group);
            return Ok(new { total = result.Readers.Count, data = result.Readers });
        }

        // 10. Chi tiết bạn đọc theo Mã sinh viên / Mã thẻ
        [HttpGet("/api/readers/{id}")]
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
                try
                {
                    var bd = _dbContext.BanDocs.FirstOrDefault(b => b.MaBanDoc == maBDInt);
                    if (bd != null && (reader = _readerService.GetReaderById(bd.MaSinhVien)) != null)
                        return Ok(new { mode = "SQL_Server", data = reader });
                }
                catch { }
            }

            // 3. Fallback từ student_profiles.json khi DB offline
            var profile = _userProfileService.GetProfile(cleanId);
            if (profile != null && !string.IsNullOrEmpty(profile.HoTen))
            {
                return Ok(new
                {
                    mode = "Offline_Fallback",
                    data = new
                    {
                        maBD = profile.MaSinhVien,
                        hoTen = profile.HoTen,
                        lopKhoa = $"{profile.LopQuanLy} · {profile.NganhHoc}",
                        soSachDangMuon = profile.BorrowedBooks?.Count ?? 0,
                        borrowTransactions = profile.BorrowedBooks
                    }
                });
            }

            return NotFound(new { message = $"Không tìm thấy bạn đọc '{cleanId}'" });
        }

        // 11. Thống kê tổng hợp Dashboard
        [HttpGet("/api/dashboard/stats")]
        public IActionResult GetDashboardStats() => Ok(_readerService.GetDashboardStatistics());
    }
}

