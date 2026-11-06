namespace BTL_LTWEB.Areas.Reader.ViewModels
{
    public class SearchIndexViewModel
    {
        // Từ khóa tìm kiếm
        public string? Keyword { get; set; }

        // Giá trị các bộ lọc đã chọn
        public string? SelectedLoai { get; set; }
        public string? SelectedTheLoai { get; set; }
        public string? SelectedNgonNgu { get; set; }
        public string? SelectedTinhTrang { get; set; }

        // Kết quả tìm kiếm
        public int TotalItems { get; set; }
        public List<BookCardItem> Results { get; set; } = new();

        // Danh sách tùy chọn cho bộ lọc
        public List<string> LoaiOptions { get; set; } = new()
        {
            "Tất cả loại học liệu",
            "Giáo trình",
            "Sách in",
            "Luận văn & Luận án",
            "Báo & Tạp chí",
            "Đề tài nghiên cứu"
        };

        public List<string> TheLoaiOptions { get; set; } = new()
        {
            "Tất cả chuyên ngành",
            "Công nghệ thông tin",
            "Kỹ thuật xây dựng & GTVT",
            "Dữ liệu & GIS",
            "Kinh tế & Logistics",
            "Kỹ thuật điện & Điện tử",
            "Khoa học cơ bản & Ngoại ngữ"
        };

        public List<string> NgonNguOptions { get; set; } = new()
        {
            "Tất cả ngôn ngữ",
            "Tiếng Việt",
            "Tiếng Anh"
        };

        public List<string> TinhTrangOptions { get; set; } = new()
        {
            "Tất cả trạng thái",
            "Có bản sẵn sàng",
            "Đang mượn hết"
        };
    }
}

