using BTL_LTWEB.Areas.Reader.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;

        public SearchController(ILogger<SearchController> logger)
        {
            _logger = logger;
        }

        // GET: /Reader/Search
        [HttpGet]
        public IActionResult Index(string? q, string? type, string? theloai, string? lang, string? status)
        {
            var allBooks = GetMockCatalog();

            var filtered = allBooks.AsEnumerable();

            // 1. Lọc theo từ khóa (tên, tác giả, mã tài liệu, thể loại)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim().ToLower();
                filtered = filtered.Where(b =>
                    b.TenTL.ToLower().Contains(kw) ||
                    b.TacGia.ToLower().Contains(kw) ||
                    b.MaTL.ToLower().Contains(kw) ||
                    b.TheLoai.ToLower().Contains(kw));
            }

            // 2. Lọc theo loại học liệu
            if (!string.IsNullOrWhiteSpace(type) && type != "Tất cả loại học liệu")
            {
                filtered = filtered.Where(b =>
                    b.DinhDang.Contains(type, StringComparison.OrdinalIgnoreCase) ||
                    b.TheLoai.Contains(type, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Lọc theo chuyên ngành / thể loại
            if (!string.IsNullOrWhiteSpace(theloai) && theloai != "Tất cả chuyên ngành")
            {
                filtered = filtered.Where(b =>
                    b.TheLoai.Contains(theloai, StringComparison.OrdinalIgnoreCase));
            }

            // 4. Lọc theo ngôn ngữ
            if (!string.IsNullOrWhiteSpace(lang) && lang != "Tất cả ngôn ngữ")
            {
                filtered = filtered.Where(b =>
                    b.NgonNgu.Equals(lang, StringComparison.OrdinalIgnoreCase));
            }

            // 5. Lọc theo tình trạng
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Có bản sẵn sàng")
                {
                    filtered = filtered.Where(b => b.SoBanSanCo > 0);
                }
                else if (status == "Đang mượn hết")
                {
                    filtered = filtered.Where(b => b.SoBanSanCo == 0);
                }
            }

            var results = filtered.ToList();

            var viewModel = new SearchIndexViewModel
            {
                Keyword = q,
                SelectedLoai = type,
                SelectedTheLoai = theloai,
                SelectedNgonNgu = lang,
                SelectedTinhTrang = status,
                TotalItems = results.Count,
                Results = results
            };

            return View(viewModel);
        }

        private List<BookCardItem> GetMockCatalog()
        {
            return new List<BookCardItem>
            {
                new BookCardItem
                {
                    MaTL = "TL-01",
                    TenTL = "Tiếng Anh chuyên ngành Quản lý xây dựng",
                    TheLoai = "NGOẠI NGỮ · XÂY DỰNG",
                    TacGia = "Nguyễn Hoàng Tùng, Vũ Minh Hằng và cộng sự",
                    DinhDang = "Giáo trình",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 14,
                    TongSoBan = 20
                },
                new BookCardItem
                {
                    MaTL = "TL-02",
                    TenTL = "Xử lý ảnh trong công nghiệp và giao thông",
                    TheLoai = "CÔNG NGHỆ · GIAO THÔNG",
                    TacGia = "Lê Thị Thúy Nga (chủ biên)",
                    DinhDang = "Sách in",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 8,
                    TongSoBan = 15
                },
                new BookCardItem
                {
                    MaTL = "TL-03",
                    TenTL = "Cơ sở GIS và ứng dụng trong giao thông vận tải",
                    TheLoai = "DỮ LIỆU & GIS",
                    TacGia = "Lê Khánh Giang (chủ biên)",
                    DinhDang = "Giáo trình",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 6,
                    TongSoBan = 12
                },
                new BookCardItem
                {
                    MaTL = "TL-04",
                    TenTL = "Quy hoạch hệ thống công trình ngầm",
                    TheLoai = "KỸ THUẬT XÂY DỰNG & GTVT",
                    TacGia = "Trần Tuấn Hiệp, Vũ Ngọc Phương (chủ biên)",
                    DinhDang = "Giáo trình",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 11,
                    TongSoBan = 18
                },
                new BookCardItem
                {
                    MaTL = "TL-05",
                    TenTL = "Lập trình Python cơ bản và nâng cao",
                    TheLoai = "CÔNG NGHỆ THÔNG TIN",
                    TacGia = "Nguyễn Văn An, Trần Thị Bình",
                    DinhDang = "Giáo trình",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 11,
                    TongSoBan = 20
                },
                new BookCardItem
                {
                    MaTL = "TL-06",
                    TenTL = "Cơ học kết cấu và công trình giao thông",
                    TheLoai = "KỸ THUẬT XÂY DỰNG & GTVT",
                    TacGia = "Phạm Duy Hòa, Đặng Gia Nải",
                    DinhDang = "Giáo trình",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 5,
                    TongSoBan = 10
                },
                new BookCardItem
                {
                    MaTL = "TL-07",
                    TenTL = "Introduction to Intelligent Transportation Systems",
                    TheLoai = "CÔNG NGHỆ THÔNG TIN",
                    TacGia = "Joseph M. Sussman",
                    DinhDang = "Sách in",
                    NgonNgu = "Tiếng Anh",
                    SoBanSanCo = 3,
                    TongSoBan = 5
                },
                new BookCardItem
                {
                    MaTL = "TL-08",
                    TenTL = "Quản trị chuỗi cung ứng và logistics quốc tế",
                    TheLoai = "KINH TẾ & LOGISTICS",
                    TacGia = "Nguyễn Hoàng Tùng, Đoàn Thị Hồng Vân",
                    DinhDang = "Sách in",
                    NgonNgu = "Tiếng Việt",
                    SoBanSanCo = 7,
                    TongSoBan = 15
                }
            };
        }
    }
}

