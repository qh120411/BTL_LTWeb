using BTL_LTWEB.Common;

namespace BTL_LTWEB.Areas.Admin.ViewModels;

public class AdminPaginationViewModel
{
    public int TotalItems { get; set; }
    public int RequestedPage { get; set; } = 1;
    public int PageSize => 10;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    public int CurrentPage => Math.Clamp(RequestedPage, 1, TotalPages);
    public int Skip => (CurrentPage - 1) * PageSize;
    public int FirstItem => TotalItems == 0 ? 0 : Skip + 1;
    public int LastItem => (int)Math.Min((long)CurrentPage * PageSize, TotalItems);
    public string Controller { get; set; } = string.Empty;
    public string Fragment { get; set; } = string.Empty;
    public string Unit { get; set; } = "mục";
    public string PageParameter { get; set; } = "page";
    public string AriaLabel { get; set; } = "Trang danh sách";
    public Dictionary<string, string?> Filters { get; set; } = new();
    public IReadOnlyList<int?> Pages => PageNavigation.GetPages(CurrentPage, TotalPages);
}
