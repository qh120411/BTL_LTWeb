using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Services;

internal static class CatalogSqlBridge
{
    public static bool Matches(BookItem catalogBook, TaiLieu document) =>
        (!string.IsNullOrWhiteSpace(catalogBook.ISBN) &&
         string.Equals(catalogBook.ISBN, document.Isbn, StringComparison.OrdinalIgnoreCase)) ||
        string.Equals(catalogBook.Title, document.TenTaiLieu, StringComparison.OrdinalIgnoreCase);

    public static void RefreshAvailability(IEnumerable<BookItem> books, QLThuVienUtcContext db)
    {
        var documents = db.TaiLieus.AsNoTracking().Include(d => d.BanSaos).ToList();
        foreach (var book in books)
        {
            var document = documents.FirstOrDefault(d => Matches(book, d));
            // A catalog title not yet used in SQL keeps its existing catalog stock.
            if (document != null)
                book.AvailableCopies = document.BanSaos.Count(c => c.TrangThai == "Có sẵn");
        }
    }

    public static void SyncCopies(IBookCatalogService catalog, QLThuVienUtcContext db, TaiLieu document)
    {
        var book = catalog.GetAll().FirstOrDefault(b => Matches(b, document));
        if (book == null) return;

        var copies = db.BanSaos.AsNoTracking().Where(c => c.MaTaiLieu == document.MaTaiLieu).ToList();
        book.AvailableCopies = copies.Count(c => c.TrangThai == "Có sẵn");
        book.TotalCopies = copies.Count;
        book.Copies = copies.Select(c => new BookCopyItem
        {
            Id = c.MaBanSao,
            BookId = book.Id,
            Barcode = c.MaVach,
            Shelf = c.ViTriKe ?? book.Shelf,
            Condition = c.TinhTrang,
            Status = c.TrangThai,
            ImportDate = c.NgayNhap?.ToString("dd/MM/yyyy") ?? string.Empty
        }).ToList();
        catalog.UpdateBook(book, out _);
    }
}
