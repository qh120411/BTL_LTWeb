namespace BTL_LTWEB.Areas.Reader.ViewModels
{
    public class HomeIndexViewModel
    {
        // 1. Tìm kiếm học liệu
        public string? SearchKeyword { get; set; }
        public string SearchTab { get; set; } = "all"; // all, print, digital

        // 2. Thẻ truy cập nhanh (4 quick links)
        public List<QuickLinkItem> QuickLinks { get; set; } = new();

        // 3. Khối tin tức và biên tập (News & Editorial)
        public List<BookCardItem> EditorialBooks { get; set; } = new();
        public NewsArticleItem? FeaturedArticle { get; set; }
        public List<AnnouncementItem> Announcements { get; set; } = new();

        // 4. Khám phá theo chủ đề (Resource Collections)
        public List<TopicItem> Topics { get; set; } = new();

        // 5. Học liệu mới (New Arrivals - Book Grid)
        public List<BookCardItem> NewArrivals { get; set; } = new();

        // 6. Dịch vụ bạn đọc (Reader Services)
        public List<ServiceShortcutItem> ReaderServices { get; set; } = new();
    }

    public class BookCardItem
    {
        public string MaTL { get; set; } = string.Empty;
        public string TenTL { get; set; } = string.Empty;
        public string TheLoai { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public string DinhDang { get; set; } = string.Empty;
        public string NgonNgu { get; set; } = string.Empty;
        public string? Anh { get; set; }
        public int SoBanSanCo { get; set; }
        public int TongSoBan { get; set; }
    }

    public class QuickLinkItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string IconSvg { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class NewsArticleItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime PublishDate { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class AnnouncementItem
    {
        public string Tag { get; set; } = string.Empty;
        public string TagType { get; set; } = "default"; // guide, catalog, survey
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class TopicItem
    {
        public string MaThL { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ServiceShortcutItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconSvg { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}

