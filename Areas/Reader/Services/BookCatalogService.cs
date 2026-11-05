using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using BTL_LTWEB.Areas.Reader.Models;

namespace BTL_LTWEB.Areas.Reader.Services
{
    public interface IBookCatalogService
    {
        IEnumerable<BookItem> GetAll();
        BookItem? GetById(int id);
        IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability);

        // Nghiệp vụ Quản lý học liệu CSDL Thư viện UTC
        bool AddBook(BookItem book, int copiesCount, out int newId, out string errorMessage);
        bool UpdateBook(BookItem book, out string errorMessage);
        bool DeleteBook(int id, out string errorMessage);
        List<BookCopyItem> GetCopiesByBookId(int bookId);
    }

    public class BookCatalogService : IBookCatalogService
    {
        private readonly List<BookItem> _books = new();
        private readonly string _storagePath;
        private readonly object _fileLock = new();

        public BookCatalogService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _storagePath = Path.Combine(dataDir, "catalog_books.json");

            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            try
            {
                lock (_fileLock)
                {
                    if (File.Exists(_storagePath))
                    {
                        var json = File.ReadAllText(_storagePath, System.Text.Encoding.UTF8);
                        var loaded = JsonSerializer.Deserialize<List<BookItem>>(json);
                        if (loaded != null && loaded.Count > 0)
                        {
                            _books.Clear();
                            _books.AddRange(loaded);
                            return;
                        }
                    }
                }
            }
            catch { }

            InitData();
            InitCopies();
            SaveToDisk();
        }

        public void SaveToDisk()
        {
            try
            {
                lock (_fileLock)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };
                    var json = JsonSerializer.Serialize(_books, options);
                    File.WriteAllText(_storagePath, json, System.Text.Encoding.UTF8);
                }
            }
            catch { }
        }

        private void InitCopies()
        {
            foreach (var b in _books)
            {
                if (b.Copies == null || b.Copies.Count == 0)
                {
                    b.Copies = new List<BookCopyItem>();
                    int total = Math.Max(1, b.TotalCopies);
                    int borrowed = Math.Max(0, total - b.AvailableCopies);

                    for (int i = 1; i <= total; i++)
                    {
                        bool isBorrowed = (i <= borrowed);
                        b.Copies.Add(new BookCopyItem
                        {
                            Id = (b.Id * 10) + i,
                            BookId = b.Id,
                            Barcode = $"UTC-{b.Id:D2}-0{i}",
                            Shelf = b.Shelf,
                            Condition = (i == 1) ? "Mới" : "Tốt",
                            Status = isBorrowed ? "Đang mượn" : "Có sẵn",
                            ImportDate = "15/01/2024"
                        });
                    }
                }
            }
        }

        public IEnumerable<BookItem> GetAll() => _books;

        public BookItem? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

        public IEnumerable<BookItem> Search(string? keyword, string? category, string? major, string? language, string? availability)
        {
            var query = _books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(kw) ||
                    b.Authors.ToLower().Contains(kw) ||
                    b.Publisher.ToLower().Contains(kw) ||
                    b.ISBN.ToLower().Contains(kw) ||
                    b.Description.ToLower().Contains(kw) ||
                    b.MajorName.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "all")
            {
                query = query.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(major) && major != "all")
            {
                query = query.Where(b => b.Major.Equals(major, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(language) && language != "all")
            {
                query = query.Where(b => b.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(availability) && availability == "available")
            {
                query = query.Where(b => b.AvailableCopies > 0);
            }

            return query.ToList();
        }

        public bool AddBook(BookItem book, int copiesCount, out int newId, out string errorMessage)
        {
            errorMessage = string.Empty;
            newId = 0;

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                errorMessage = "Tiêu đề học liệu không được để trống!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(book.Authors))
            {
                errorMessage = "Tên tác giả không được để trống!";
                return false;
            }

            newId = _books.Count > 0 ? _books.Max(b => b.Id) + 1 : 1;
            book.Id = newId;
            book.TotalCopies = Math.Max(1, copiesCount);
            book.AvailableCopies = book.TotalCopies;
            if (string.IsNullOrWhiteSpace(book.CoverImage))
            {
                book.CoverImage = "/images/covers/sach_01.jpg";
            }
            if (string.IsNullOrWhiteSpace(book.Shelf))
            {
                book.Shelf = "Kệ IT-01";
            }
            if (string.IsNullOrWhiteSpace(book.ISBN))
            {
                book.ISBN = $"978-604-{(newId * 17) % 90 + 10}-{(newId * 31) % 9000 + 1000}-{newId % 9}";
            }
            if (string.IsNullOrWhiteSpace(book.Publisher))
            {
                book.Publisher = "NXB Đại học Giao thông Vận tải";
            }

            // Tạo các bản sao vật lý nhập kho
            book.Copies = new List<BookCopyItem>();
            for (int i = 1; i <= book.TotalCopies; i++)
            {
                book.Copies.Add(new BookCopyItem
                {
                    Id = (newId * 10) + i,
                    BookId = newId,
                    Barcode = $"UTC-{newId:D2}-0{i}",
                    Shelf = book.Shelf,
                    Condition = "Mới",
                    Status = "Có sẵn",
                    ImportDate = DateTime.Today.ToString("dd/MM/yyyy")
                });
            }

            _books.Insert(0, book);
            SaveToDisk();
            return true;
        }

        public bool UpdateBook(BookItem book, out string errorMessage)
        {
            errorMessage = string.Empty;
            var existing = _books.FirstOrDefault(b => b.Id == book.Id);
            if (existing == null)
            {
                errorMessage = "Không tìm thấy học liệu cần cập nhật trong CSDL!";
                return false;
            }

            existing.Title = book.Title;
            existing.Authors = book.Authors;
            existing.Publisher = book.Publisher;
            existing.Major = book.Major;
            existing.MajorName = book.MajorName;
            existing.Year = book.Year;
            existing.Pages = book.Pages;
            existing.ISBN = book.ISBN;
            existing.Shelf = book.Shelf;
            if (!string.IsNullOrWhiteSpace(book.CoverImage))
            {
                existing.CoverImage = book.CoverImage;
            }
            existing.Description = book.Description;

            // Cập nhật vị trí kệ trên tất cả bản sao
            foreach (var c in existing.Copies)
            {
                c.Shelf = existing.Shelf;
            }

            SaveToDisk();
            return true;
        }

        public bool DeleteBook(int id, out string errorMessage)
        {
            errorMessage = string.Empty;
            var existing = _books.FirstOrDefault(b => b.Id == id);
            if (existing == null)
            {
                errorMessage = "Không tìm thấy học liệu trong CSDL!";
                return false;
            }

            if (existing.Copies.Any(c => c.Status == "Đang mượn") || existing.AvailableCopies < existing.TotalCopies)
            {
                errorMessage = $"Không thể xóa học liệu '{existing.Title}' vì hiện có {existing.TotalCopies - existing.AvailableCopies} bản sao đang được sinh viên mượn lưu hành ngoài kho!";
                return false;
            }

            _books.Remove(existing);
            SaveToDisk();
            return true;
        }

        public List<BookCopyItem> GetCopiesByBookId(int bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            return book?.Copies ?? new List<BookCopyItem>();
        }

        private void InitData()
        {
            _books.Add(new BookItem
            {
                Id = 1,
                Title = "Cấu trúc dữ liệu và giải thuật (Python: Thuật toán & Cấu trúc dữ liệu)",
                Authors = "Bùi Việt Hà",
                Publisher = "NXB Đại Học Quốc Gia Hà Nội",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 506,
                ISBN = "978-604-0-32451-2",
                CoverImage = "/images/covers/sach_01.jpg",
                AvailableCopies = 3,
                TotalCopies = 4,
                Shelf = "Kệ IT-01",
                Description = "Giáo trình trang bị nền tảng chuyên sâu về thuật toán và các cấu trúc dữ liệu kinh điển: mảng động, danh sách liên kết, ngăn xếp (Stack), hàng đợi (Queue), cây nhị phân, bảng băm (Hash Table) và đồ thị, ứng dụng trực tiếp bằng ngôn ngữ hiện đại.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 2,
                Title = "Giáo trình Cơ sở dữ liệu quan hệ - Lý thuyết và Thực hành",
                Authors = "Phạm Minh Chuẩn, Trịnh Thị Nhị, Nguyễn Văn Quyết",
                Publisher = "NXB Khoa Học Và Kỹ Thuật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 328,
                ISBN = "978-604-67-2103-1",
                CoverImage = "/images/covers/sach_02.jpg",
                AvailableCopies = 3,
                TotalCopies = 4,
                Shelf = "Kệ IT-01",
                Description = "Trình bày có hệ thống mô hình thực thể liên kết (ERD), đại số quan hệ, phụ thuộc hàm, các dạng chuẩn chuẩn hóa dữ liệu (1NF, 2NF, 3NF, BCNF) cùng kỹ thuật thiết kế và truy vấn SQL tối ưu trên hệ quản trị CSDL.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 3,
                Title = "Giáo trình Mạng máy tính",
                Authors = "Nguyễn Thế Lộc, Nguyễn Hồng Phương",
                Publisher = "NXB Đại Học Sư Phạm",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 272,
                ISBN = "978-604-54-8921-0",
                CoverImage = "/images/covers/sach_03.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ IT-02",
                Description = "Tài liệu giảng dạy chuẩn mực về kiến trúc mạng phân tầng OSI và TCP/IP, các giao thức tầng ứng dụng (HTTP, DNS), giao vận (TCP, UDP), mạng (IP, định tuyến OSPF/BGP) và kỹ thuật an toàn thông tin cơ bản trong môi trường truyền số liệu.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 4,
                Title = "Giáo trình Cơ sở An toàn thông tin",
                Authors = "Học viện Công nghệ Bưu chính Viễn thông",
                Publisher = "NXB Thông Tin Và Truyền Thông",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2021,
                Pages = 350,
                ISBN = "978-604-80-4512-8",
                CoverImage = "/images/covers/sach_04.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ IT-02",
                Description = "Giáo trình cốt lõi trang bị các nguyên lý bảo mật hệ thống: mã hóa đối xứng (AES, DES), mã hóa bất đối xứng (RSA, ECC), chữ ký số, hàm băm mật mã (SHA), xác thực người dùng và các chính sách phòng chống tấn công mạng.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 5,
                Title = "Giáo trình Kỹ thuật lập trình C căn bản và nâng cao",
                Authors = "Phạm Văn Ất, Nguyễn Hiếu Cường",
                Publisher = "NXB Giao Thông Vận Tải",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2021,
                Pages = 440,
                ISBN = "978-604-67-1950-2",
                CoverImage = "/images/covers/sach_05.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ IT-03",
                Description = "Tác phẩm kinh điển của GS. Phạm Văn Ất hướng dẫn kỹ thuật lập trình từ cơ bản đến nâng cao: cấu trúc điều khiển, mảng, con trỏ (pointer), cấp phát động bộ nhớ, xử lý tệp tin và các kỹ thuật giải thuật tối ưu trên ngôn ngữ C.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 6,
                Title = "Giáo trình C++ và Lập trình hướng đối tượng",
                Authors = "Phạm Văn Ất, Lê Trường Thông",
                Publisher = "NXB Khoa Học Và Kỹ Thuật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 488,
                ISBN = "978-604-67-1845-1",
                CoverImage = "/images/covers/sach_06.jpg",
                AvailableCopies = 3,
                TotalCopies = 4,
                Shelf = "Kệ IT-03",
                Description = "Cung cấp nền tảng vững chắc về tư duy hướng đối tượng (OOP): lớp và đối tượng (Class & Object), tính đóng gói, kế thừa, đa hình, hàm bạn, nạp chồng toán tử (operator overloading), khuôn hình (Template) và thư viện chuẩn STL.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 7,
                Title = "Giáo trình Trí tuệ nhân tạo, Học máy và Học sâu",
                Authors = "Nguyễn Tuấn Đăng",
                Publisher = "NXB Khoa Học Và Kỹ Thuật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 380,
                ISBN = "978-604-67-2415-5",
                CoverImage = "/images/covers/sach_07.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ IT-04",
                Description = "Tài liệu học thuật hiện đại trình bày các phương pháp tìm kiếm không gian trạng thái, thuật toán học máy có giám sát và không giám sát (SVM, Random Forest, K-Means), và mạng nơ-ron tích chập (CNN, RNN) trong xử lý hình ảnh và ngôn ngữ tự nhiên.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 8,
                Title = "Giáo trình Kiến trúc máy tính và Hợp ngữ",
                Authors = "Phạm Văn Cường",
                Publisher = "NXB Bách Khoa Hà Nội",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 360,
                ISBN = "978-604-938-634-3",
                CoverImage = "/images/covers/sach_08.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ IT-04",
                Description = "Nghiên cứu cấu trúc phần cứng máy tính: tập lệnh kiến trúc x86/ARM, ALU, đơn vị điều khiển (CU), hệ thống nhớ phân cấp (Cache, RAM), đường ống xử lý lệnh (Pipelining) và kỹ thuật lập trình hợp ngữ Assembly cơ bản.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 9,
                Title = "Giáo trình Kỹ nghệ phần mềm (Công nghệ phần mềm)",
                Authors = "Đoàn Văn Ban",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2021,
                Pages = 310,
                ISBN = "978-604-0-28731-2",
                CoverImage = "/images/covers/sach_09.jpg",
                AvailableCopies = 3,
                TotalCopies = 4,
                Shelf = "Kệ IT-05",
                Description = "Trang bị quy trình phát triển phần mềm chuyên nghiệp: phân tích yêu cầu (SRS), mô hình hóa UML, thiết kế kiến trúc hệ thống, quy trình Agile/Scrum, kiểm thử tự động (Unit Test, Integration Test) và quản trị dự án phần mềm.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 10,
                Title = "Giáo trình Lập trình Python cơ bản và ứng dụng",
                Authors = "Đậu Hải Long",
                Publisher = "NXB Thông Tin Và Truyền Thông",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "cntt",
                MajorName = "Công nghệ thông tin",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 320,
                ISBN = "978-604-80-5120-4",
                CoverImage = "/images/covers/sach_10.jpg",
                AvailableCopies = 3,
                TotalCopies = 4,
                Shelf = "Kệ IT-05",
                Description = "Hướng dẫn thực hành ngôn ngữ Python từ cú pháp cơ bản, xử lý ngoại lệ, thao tác file, đến ứng dụng phân tích dữ liệu kỹ thuật với thư viện NumPy, Pandas, Matplotlib và phát triển web với Django/Flask.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 11,
                Title = "Toán học cao cấp - Tập 1: Đại số và Hình học giải tích",
                Authors = "Nguyễn Đình Trí, Trần Việt Dũng, Trần Xuân Hiển",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "toanly",
                MajorName = "Khoa học cơ bản & Toán",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2024,
                Pages = 296,
                ISBN = "978-604-0-34120-5",
                CoverImage = "/images/covers/sach_11.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ CB-01",
                Description = "Bộ giáo trình chuẩn quốc gia dành cho sinh viên kỹ thuật: lý thuyết định thức, ma trận, hệ phương trình đại số tuyến tính, không gian véc tơ, ánh xạ tuyến tính, dạng toàn phương và hình học giải tích trong không gian.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 12,
                Title = "Toán học cao cấp - Tập 2: Giải tích một biến và nhiều biến",
                Authors = "Nguyễn Đình Trí, Trần Việt Dũng, Trần Xuân Hiển",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "toanly",
                MajorName = "Khoa học cơ bản & Toán",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2024,
                Pages = 384,
                ISBN = "978-604-0-34121-2",
                CoverImage = "/images/covers/sach_12.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ CB-01",
                Description = "Nội dung giải tích nền tảng: giới hạn và liên tục của hàm số, phép tính vi phân hàm một biến và nhiều biến, tích phân bất định, tích phân xác định và ứng dụng hình học, vật lý trong kỹ thuật.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 13,
                Title = "Toán học cao cấp - Tập 3: Chuỗi và Phương trình vi phân",
                Authors = "Nguyễn Đình Trí, Trần Việt Dũng, Trần Xuân Hiển",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "toanly",
                MajorName = "Khoa học cơ bản & Toán",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2024,
                Pages = 340,
                ISBN = "978-604-0-34122-9",
                CoverImage = "/images/covers/sach_13.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ CB-02",
                Description = "Nghiên cứu chuỗi số, chuỗi hàm, chuỗi lũy thừa và chuỗi Fourier; phương trình vi phân cấp 1, phương trình vi phân tuyến tính cấp 2 hệ số hằng và hệ phương trình vi phân ứng dụng mô hình hóa dao động cơ học.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 14,
                Title = "Giáo trình Xác suất và Thống kê toán học",
                Authors = "Tống Đình Quỳ",
                Publisher = "NXB Bách Khoa Hà Nội",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "toanly",
                MajorName = "Khoa học cơ bản & Toán",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 316,
                ISBN = "978-604-938-890-3",
                CoverImage = "/images/covers/sach_14.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ CB-02",
                Description = "Cung cấp lý thuyết biến ngẫu nhiên rời rạc và liên tục, các quy luật phân phối chuẩn, Poisson, nhị thức; lý thuyết ước lượng tham số, kiểm định giả thuyết thống kê, phân tích phương sai và hồi quy tương quan.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 15,
                Title = "Vật lý đại cương - Tập 1: Cơ học và Nhiệt học",
                Authors = "Lương Duyên Bình",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "toanly",
                MajorName = "Khoa học cơ bản & Toán",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 280,
                ISBN = "978-604-0-29105-0",
                CoverImage = "/images/covers/sach_15.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ CB-03",
                Description = "Giáo trình vật lý đại cương của GS. Lương Duyên Bình: động học chất điểm, động lực học chất điểm và vật rắn, các định luật bảo toàn (cơ năng, động lượng), thuyết động học chất khí và hai nguyên lý nhiệt động lực học.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 16,
                Title = "Vật lý đại cương - Tập 2: Điện học và Từ học",
                Authors = "Lương Duyên Bình",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "toanly",
                MajorName = "Khoa học cơ bản & Toán",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 320,
                ISBN = "978-604-0-29106-7",
                CoverImage = "/images/covers/sach_16.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ CB-03",
                Description = "Trình bày trường tĩnh điện, định luật Gauss, điện thế, vật dẫn trong điện trường, từ trường của dòng điện không đổi, định luật Ampe, hiện tượng cảm ứng điện từ và hệ phương trình Maxwell về trường điện từ.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 17,
                Title = "Giáo trình Sức bền vật liệu - Tập 1",
                Authors = "Nguyễn Văn Vượng, Lê Ngọc Hồng",
                Publisher = "NXB Giao Thông Vận Tải",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 364,
                ISBN = "978-604-67-1520-7",
                CoverImage = "/images/covers/sach_17.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-01",
                Description = "Học phần then chốt của kỹ sư xây dựng, giao thông và cơ khí: trạng thái ứng suất và biến dạng, kéo nén đúng tâm, xoắn thuần túy thanh tiết diện tròn, uốn phẳng của dầm và các thuyết bền phá hủy vật liệu.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 18,
                Title = "Cơ học kỹ thuật - Tập 1: Tĩnh học và Động học",
                Authors = "Đỗ Sanh",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 312,
                ISBN = "978-604-0-31204-5",
                CoverImage = "/images/covers/sach_18.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-01",
                Description = "Cung cấp các tiên đề tĩnh học, thu gọn hệ lực không gian, điều kiện cân bằng của hệ lực phẳng và không gian, ma sát; động học điểm và động học vật rắn, chuyển động phức tạp của điểm và cơ cấu truyền động.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 19,
                Title = "Giáo trình Nền và Móng công trình",
                Authors = "Vũ Công Ngữ, Nguyễn Văn Quảng",
                Publisher = "NXB Xây Dựng",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2021,
                Pages = 340,
                ISBN = "978-604-82-4102-1",
                CoverImage = "/images/covers/sach_19.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-02",
                Description = "Hướng dẫn tính toán và thiết kế móng nông, móng cọc đài thấp, móng cọc đài cao chịu tải trọng lệch tâm, tính toán độ lún nền đất, sức chịu tải giới hạn của đất nền và các giải pháp gia cố nền đất yếu.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 20,
                Title = "Giáo trình Thiết kế hình học đường ô tô và đường cao tốc",
                Authors = "Dương Học Hải, Nguyễn Quang Chiêu",
                Publisher = "NXB Giao Thông Vận Tải",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 420,
                ISBN = "978-604-67-1755-3",
                CoverImage = "/images/covers/sach_20.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-02",
                Description = "Tài liệu đào tạo chuyên sâu ngành Cầu đường UTC: nguyên lý vạch tuyến đường, thiết kế bình đồ, trắc dọc, trắc ngang, đường cong chuyển tiếp, tầm nhìn xe chạy và bố trí các nút giao thông giao cắt lập thể.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 21,
                Title = "Kết cấu bê tông cốt thép - Nguyên lý thiết kế",
                Authors = "Nguyễn Đình Cống",
                Publisher = "NXB Xây Dựng",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 450,
                ISBN = "978-604-82-5301-7",
                CoverImage = "/images/covers/sach_21.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-03",
                Description = "Trang bị phương pháp tính toán cấu kiện bê tông cốt thép theo trạng thái giới hạn (TCVN 5574): tính toán chịu uốn trên tiết diện thẳng góc và nghiêng, chịu nén đúng tâm và lệch tâm, độ võng và bề rộng vết nứt.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 22,
                Title = "Giáo trình Cơ sở Kỹ thuật nhiệt",
                Authors = "Bùi Hải, Trần Thế Sơn",
                Publisher = "NXB Giáo Dục Việt Nam",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 368,
                ISBN = "978-604-0-30514-6",
                CoverImage = "/images/covers/sach_22.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-03",
                Description = "Nghiên cứu quá trình nhiệt động của khí lý tưởng, chu trình nhiệt động của động cơ đốt trong, turbine khí và máy lạnh; lý thuyết truyền nhiệt: dẫn nhiệt ổn định, trao đổi nhiệt đối lưu và bức xạ nhiệt công nghiệp.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 23,
                Title = "Giáo trình Nguyên lý máy",
                Authors = "Lê Cung",
                Publisher = "NXB Khoa Học Và Kỹ Thuật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "xaydung",
                MajorName = "Kỹ thuật công trình & Giao thông",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2022,
                Pages = 350,
                ISBN = "978-604-67-2015-8",
                CoverImage = "/images/covers/sach_23.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ GT-04",
                Description = "Cấu tạo và xếp loại cơ cấu, phân tích động học và lực học cơ cấu phẳng bằng phương pháp đồ họa và giải tích, ma sát trong khớp động, cân bằng cơ cấu và động lực học máy.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 24,
                Title = "Nguyên lý Quản trị chuỗi cung ứng và Logistics",
                Authors = "Michael H. Hugos, Đặng Đình Đào",
                Publisher = "NXB Lao Động",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "kinhte",
                MajorName = "Kinh tế & Logistics",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 396,
                ISBN = "978-604-59-9812-4",
                CoverImage = "/images/covers/sach_24.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ KT-01",
                Description = "Cẩm nang toàn diện về 5 khâu cốt lõi của chuỗi cung ứng: Sản xuất, Tồn kho, Địa điểm, Vận tải và Thông tin. Phân tích tối ưu hóa logistics cảng biển, kho bãi và hệ thống phân phối đa phương thức.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 25,
                Title = "Giáo trình Kinh tế vi mô",
                Authors = "Vũ Trọng Phong, Trần Thị Hoa",
                Publisher = "NXB Đại Học Quốc Gia Hà Nội",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "kinhte",
                MajorName = "Kinh tế & Logistics",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 345,
                ISBN = "978-604-99-4215-0",
                CoverImage = "/images/covers/sach_25.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ KT-01",
                Description = "Trang bị lý thuyết kinh tế học cơ sở: cung - cầu và độ co giãn, lý thuyết hành vi người tiêu dùng, tối ưu hóa sản xuất và chi phí doanh nghiệp, cấu trúc thị trường cạnh tranh hoàn hảo, độc quyền và cạnh tranh độc quyền.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 26,
                Title = "Giáo trình Kinh tế vĩ mô",
                Authors = "Nguyễn Văn Công",
                Publisher = "NXB Đại Học Quốc Gia Hà Nội",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "kinhte",
                MajorName = "Kinh tế & Logistics",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 380,
                ISBN = "978-604-99-4216-7",
                CoverImage = "/images/covers/sach_26.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ KT-02",
                Description = "Nghiên cứu hoạt động nền kinh tế tổng thể: đo lường sản lượng quốc gia (GDP/GNP), lạm phát, thất nghiệp, mô hình tổng cung - tổng cầu (AS-AD), thị trường tiền tệ, chính sách tài khóa và tiền tệ của Ngân hàng Trung ương.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 27,
                Title = "Giáo trình Nguyên lý kế toán",
                Authors = "Nguyễn Thị Loan",
                Publisher = "NXB Tài Chính",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "kinhte",
                MajorName = "Kinh tế & Logistics",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 410,
                ISBN = "978-604-79-2810-1",
                CoverImage = "/images/covers/sach_27.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ KT-02",
                Description = "Giáo trình nền tảng kế toán tài chính: đối tượng và phương pháp kế toán, chứng từ và kiểm kê tài sản, tài khoản và ghi sổ kép, tính giá các đối tượng kế toán, và quy trình lập Báo cáo tài chính chuẩn mực.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 28,
                Title = "Giáo trình Triết học Mác - Lênin (Dành cho bậc đại học hệ không chuyên lý luận chính trị)",
                Authors = "Bộ Giáo Dục Và Đào Tạo",
                Publisher = "NXB Chính Trị Quốc Gia Sự Thật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "chinhtri",
                MajorName = "Lý luận chính trị & Pháp luật",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 492,
                ISBN = "978-604-57-7120-9",
                CoverImage = "/images/covers/sach_28.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ LL-01",
                Description = "Giáo trình chuẩn quốc gia ban hành theo chương trình mới của Bộ GD&ĐT: Triết học và vai trò của triết học trong đời sống xã hội; Chủ nghĩa duy vật biện chứng (vật chất và ý thức, phép biện chứng duy vật); Chủ nghĩa duy vật lịch sử.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 29,
                Title = "Giáo trình Kinh tế chính trị Mác – Lênin (Dành cho bậc đại học)",
                Authors = "Bộ Giáo Dục Và Đào Tạo",
                Publisher = "NXB Chính Trị Quốc Gia Sự Thật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "chinhtri",
                MajorName = "Lý luận chính trị & Pháp luật",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 360,
                ISBN = "978-604-57-7121-6",
                CoverImage = "/images/covers/sach_29.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ LL-01",
                Description = "Cung cấp lý luận hàng hóa, thị trường và vai trò của các chủ thể tham gia thị trường; sản xuất giá trị thặng dư; cạnh tranh và độc quyền trong nền kinh tế thị trường; kinh tế thị trường định hướng XHCN tại Việt Nam.",
                IsOnline = false
            });
            _books.Add(new BookItem
            {
                Id = 30,
                Title = "Giáo trình Tư tưởng Hồ Chí Minh (Dành cho bậc đại học)",
                Authors = "Bộ Giáo Dục Và Đào Tạo",
                Publisher = "NXB Chính Trị Quốc Gia Sự Thật",
                Category = "books",
                CategoryName = "Sách & Giáo trình",
                Major = "chinhtri",
                MajorName = "Lý luận chính trị & Pháp luật",
                Language = "vie",
                LanguageName = "Tiếng Việt",
                Year = 2023,
                Pages = 320,
                ISBN = "978-604-57-7122-3",
                CoverImage = "/images/covers/sach_30.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                Shelf = "Kệ LL-02",
                Description = "Hệ thống hóa nguồn gốc, quá trình hình thành và phát triển tư tưởng Hồ Chí Minh: tư tưởng về độc lập dân tộc và chủ nghĩa xã hội, đại đoàn kết toàn dân tộc, xây dựng Nhà nước của nhân dân, do nhân dân, vì nhân dân và đạo đức cách mạng.",
                IsOnline = false
            });
        }
    }
}
