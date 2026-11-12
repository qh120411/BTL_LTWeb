namespace BTL_LTWEB.Areas.Reader.Models
{
    public class BookItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Authors { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = "books"; // all, books, thesis, journal
        public string CategoryName { get; set; } = "Sách & Giáo trình";
        public string Major { get; set; } = string.Empty; // all, cntt, xaydung, kinhte, toanly, chinhtri
        public string MajorName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty; // all, vie, eng
        public string LanguageName { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
        public int Pages { get; set; } = 0;
        public string ISBN { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public int AvailableCopies { get; set; } = 0;
        public int TotalCopies { get; set; } = 0;
        public string Shelf { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public string DocumentCode => Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public int BorrowedCopies => Copies.Count(c => c.Status == "Đang mượn" && c.Condition != "Mất" && c.Condition != "Hỏng");
        public string StockBadgeText => IsOnline ? "Bản điện tử" : $"{AvailableCopies} bản sẵn có · Sách in";

        // Danh sách các bản sao vật lý trong kho (CSDL Thật)
        public List<BookCopyItem> Copies { get; set; } = new();
    }

    public class BookCopyItem
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Barcode { get; set; } = string.Empty; // Mã vạch: UTC-01-01
        public string Shelf { get; set; } = string.Empty;   // Vị trí kệ: Kệ IT-01
        public string Condition { get; set; } = string.Empty;      // Tình trạng: Mới, Tốt, Rách nhẹ
        public string Status { get; set; } = string.Empty;      // Trạng thái: Có sẵn, Đang mượn
        public string ImportDate { get; set; } = string.Empty;
        public DateOnly? DueDate { get; set; }
        public bool IsAvailable => Status == "Có sẵn" && Condition != "Mất" && Condition != "Hỏng";
    }

    public class BookSearchViewModel
    {
        public string Keyword { get; set; } = string.Empty;
        public string Category { get; set; } = "all";
        public string Major { get; set; } = "all";
        public string Language { get; set; } = "all";
        public string Availability { get; set; } = "all";
        public List<BookItem> Items { get; set; } = new();
        public int TotalCount => Items.Count;
    }
}
