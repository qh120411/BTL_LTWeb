using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Services;

public interface IReaderService
{
    ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup);
    Task<List<BorrowReaderSearchItem>> SearchBorrowReadersAsync(string? query, CancellationToken cancellationToken = default);
    ReaderDetailViewModel? GetReaderById(string id);
    bool UpdateReaderSettings(ReaderDetailViewModel model);
    ImportQldtViewModel GetImportPreview();
    bool ProcessImport(out int updatedCount, out int insertedCount);

    // Mượn trả & Bồi thường tài liệu học liệu
    bool ReturnBook(string maBD, string maGD, out string bookTitle, out decimal reducedFine, string? staffUsername = null);
    bool BorrowBook(string maBD, int bookId, out string bookTitle, out string errorMessage, string? staffUsername = null);
    bool ProcessReturnOrCompensation(string maBD, string maGD, string actionType, string compensationType, decimal compensationAmount, bool payOverdue, string? note, out string message, string? staffUsername = null);

    // Bổ sung các phương thức lấy dữ liệu mượn trả, tài chính & quá hạn thật theo CSDL
    List<CirculationTransactionItem> GetAllCirculationTransactions();
    List<OverdueItemViewModel> GetAllOverdueLoans();
    DashboardViewModel GetDashboardStatistics();

    // Sổ quỹ tài chính Thư viện UTC
    FinancialSummaryViewModel GetFinancialSummary();
    List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? searchKeyword, string? selectedType);
}

public interface IReportService
{
    ReportViewModel GetReport(string? period, string? search = null, int page = 1, DateOnly? startDate = null, DateOnly? endDate = null);
    byte[] GenerateExcelReport(string? period, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null);
}
