using BTL_LTWEB.Areas.Reader.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeIndexViewModel
            {
                SearchTab = "all",

                // 1. Thẻ truy cập nhanh (4 quick links)
                QuickLinks = new List<QuickLinkItem>
                {
                    new QuickLinkItem
                    {
                        Title = "Thư viện của tôi",
                        Subtitle = "Sách đang mượn & hạn trả",
                        IconSvg = "book-shelf",
                        Url = "/Reader/MyLibrary"
                    },
                    new QuickLinkItem
                    {
                        Title = "Hướng dẫn mượn trả",
                        Subtitle = "Thông tin cần biết",
                        IconSvg = "guide",
                        Url = "/Reader/Guide"
                    },
                    new QuickLinkItem
                    {
                        Title = "Giờ phục vụ",
                        Subtitle = "Lịch mở cửa các phòng",
                        IconSvg = "clock",
                        Url = "/Reader/Guide#hours"
                    },
                    new QuickLinkItem
                    {
                        Title = "Hỗ trợ bạn đọc",
                        Subtitle = "Thư viện luôn sẵn sàng",
                        IconSvg = "support",
                        Url = "/Reader/Guide#support"
                    }
                },

                // 2. Học liệu đáng chú ý (Editorial column)
                EditorialBooks = new List<BookCardItem>
                {
                    new BookCardItem
                    {
                        MaTL = "TLVN26-001",
                        TenTL = "Nhập môn lập trình Python",
                        TheLoai = "CÔNG NGHỆ THÔNG TIN",
                        TacGia = "Nguyễn Văn An, Trần Thị Bình",
                        DinhDang = "Giáo trình",
                        NgonNgu = "Tiếng Việt",
                        SoBanSanCo = 11,
                        TongSoBan = 20
                    },
                    new BookCardItem
                    {
                        MaTL = "TLVN26-002",
                        TenTL = "Cơ học kết cấu & công trình",
                        TheLoai = "KỸ THUẬT XÂY DỰNG",
                        TacGia = "Trần Thị Bình",
                        DinhDang = "Giáo trình",
                        NgonNgu = "Tiếng Việt",
                        SoBanSanCo = 8,
                        TongSoBan = 15
                    },
                    new BookCardItem
                    {
                        MaTL = "TLGB26-001",
                        TenTL = "Introduction to Data Science",
                        TheLoai = "KHOA HỌC DỮ LIỆU",
                        TacGia = "John Smith",
                        DinhDang = "Sách in",
                        NgonNgu = "Tiếng Anh",
                        SoBanSanCo = 2,
                        TongSoBan = 5
                    }
                },

                // 3. Bài viết nổi bật (Featured Article)
                FeaturedArticle = new NewsArticleItem
                {
                    Id = 1,
                    Category = "Hoạt động thư viện · Không gian học tập",
                    Title = "Khám phá không gian học tập và nghiên cứu hiện đại tại thư viện UTC",
                    Summary = "Thư viện trường Đại học Giao thông Vận tải trang bị hệ thống phòng đọc yên tĩnh, không gian mở học nhóm, wifi tốc độ cao cùng hàng chục ngàn đầu sách giáo trình, luận văn và tài liệu số phục vụ bạn đọc.",
                    ImageUrl = "/images/library-banner.jpg",
                    PublishDate = DateTime.Now.AddDays(-2),
                    Url = "/Reader/News/Detail/1"
                },

                // 4. Thông báo mới nhất (Announcements)
                Announcements = new List<AnnouncementItem>
                {
                    new AnnouncementItem
                    {
                        Tag = "HƯỚNG DẪN",
                        TagType = "guide",
                        Title = "Bắt đầu hành trình sử dụng thư viện UTC cho tân sinh viên",
                        Url = "/Reader/News/Detail/guide-freshman"
                    },
                    new AnnouncementItem
                    {
                        Tag = "HỌC LIỆU",
                        TagType = "catalog",
                        Title = "Giới thiệu bộ giáo trình và học liệu chuyên ngành mới trong học kỳ",
                        Url = "/Reader/News/Detail/new-materials"
                    },
                    new AnnouncementItem
                    {
                        Tag = "KHẢO SÁT",
                        TagType = "survey",
                        Title = "Lắng nghe ý kiến của bạn đọc nhằm nâng cao chất lượng phục vụ",
                        Url = "/Reader/News/Detail/survey-feedback"
                    }
                },

                // 5. Khám phá theo chủ đề (Resource Collections)
                Topics = new List<TopicItem>
                {
                    new TopicItem
                    {
                        MaThL = "THL001",
                        Title = "Công nghệ thông tin",
                        Description = "Lập trình, AI, mạng máy tính, an toàn thông tin & khoa học dữ liệu",
                        Url = "/Reader/Search?theloai=THL001"
                    },
                    new TopicItem
                    {
                        MaThL = "THL002",
                        Title = "Kỹ thuật xây dựng & GTVT",
                        Description = "Cầu đường, công trình ngầm, quy hoạch giao thông & logistics",
                        Url = "/Reader/Search?theloai=THL002"
                    },
                    new TopicItem
                    {
                        MaThL = "THL003",
                        Title = "Khoa học cơ bản & Ngoại ngữ",
                        Description = "Toán giải tích, xác suất thống kê, vật lý đại cương & tiếng Anh chuyên ngành",
                        Url = "/Reader/Search?theloai=THL003"
                    },
                    new TopicItem
                    {
                        MaThL = "THL004",
                        Title = "Kinh tế & Quản trị logistics",
                        Description = "Kinh tế vận tải, quản lý chuỗi cung ứng, thương mại quốc tế",
                        Url = "/Reader/Search?theloai=THL004"
                    }
                },

                // 6. Học liệu mới (New arrivals - 4 cards)
                NewArrivals = new List<BookCardItem>
                {
                    new BookCardItem
                    {
                        MaTL = "TLVN26-001",
                        TenTL = "Lập trình Python cơ bản",
                        TheLoai = "CÔNG NGHỆ THÔNG TIN",
                        TacGia = "Nguyễn Văn An, Trần Thị Bình",
                        DinhDang = "Giáo trình",
                        NgonNgu = "Tiếng Việt",
                        SoBanSanCo = 11,
                        TongSoBan = 20
                    },
                    new BookCardItem
                    {
                        MaTL = "TLVN26-002",
                        TenTL = "Cơ sở dữ liệu",
                        TheLoai = "CÔNG NGHỆ THÔNG TIN",
                        TacGia = "Trần Thị Bình",
                        DinhDang = "Giáo trình",
                        NgonNgu = "Tiếng Việt",
                        SoBanSanCo = 8,
                        TongSoBan = 15
                    },
                    new BookCardItem
                    {
                        MaTL = "TLVN26-003",
                        TenTL = "Kỹ thuật điện và điện tử ứng dụng",
                        TheLoai = "KỸ THUẬT ĐIỆN",
                        TacGia = "Lê Thị Thúy Nga",
                        DinhDang = "Giáo trình",
                        NgonNgu = "Tiếng Việt",
                        SoBanSanCo = 9,
                        TongSoBan = 12
                    },
                    new BookCardItem
                    {
                        MaTL = "TLVN26-004",
                        TenTL = "Quản trị logistics & Chuỗi cung ứng",
                        TheLoai = "LOGISTICS",
                        TacGia = "Nguyễn Hoàng Tùng",
                        DinhDang = "Giáo trình",
                        NgonNgu = "Tiếng Việt",
                        SoBanSanCo = 10,
                        TongSoBan = 18
                    }
                },

                // 7. Dịch vụ bạn đọc
                ReaderServices = new List<ServiceShortcutItem>
                {
                    new ServiceShortcutItem
                    {
                        Title = "Mượn và trả sách tại quầy",
                        Description = "Thủ tục nhanh chóng, tiện lợi bằng thẻ sinh viên hoặc căn cước công dân gắn chip.",
                        IconSvg = "book-check",
                        Url = "/Reader/Guide#muon-tra"
                    },
                    new ServiceShortcutItem
                    {
                        Title = "Gia hạn tài liệu trực tuyến",
                        Description = "Chủ động kiểm tra hạn trả và gửi yêu cầu gia hạn sách mọi lúc qua cổng bạn đọc OPAC.",
                        IconSvg = "calendar-clock",
                        Url = "/Reader/MyLibrary"
                    },
                    new ServiceShortcutItem
                    {
                        Title = "Không gian tự học & phòng nhóm",
                        Description = "Trang bị đầy đủ wifi tốc độ cao, máy tính tra cứu tự do và phòng thảo luận theo nhóm.",
                        IconSvg = "users",
                        Url = "/Reader/Guide#phong-doc"
                    }
                }
            };

            return View(viewModel);
        }
    }
}

