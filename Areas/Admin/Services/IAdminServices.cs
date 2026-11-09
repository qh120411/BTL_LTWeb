using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Services;

public interface IReaderService
{
    ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup);
    ReaderDetailViewModel? GetReaderById(string id);
    bool UpdateReaderSettings(ReaderDetailViewModel model);
    ImportQldtViewModel GetImportPreview();
    bool ProcessImport(out int updatedCount, out int insertedCount);

    // Mượn trả & Bồi thường tài liệu học liệu
    bool ReturnBook(string maBD, string maGD, out string bookTitle, out decimal reducedFine);
    bool BorrowBook(string maBD, int bookId, out string bookTitle, out string errorMessage);
    bool ProcessReturnOrCompensation(string maBD, string maGD, string actionType, string compensationType, decimal compensationAmount, bool payOverdue, string? note, out string message);

    // Bổ sung các phương thức lấy dữ liệu mượn trả, tài chính & quá hạn thật theo CSDL
    List<CirculationTransactionItem> GetAllCirculationTransactions();
    Dictionary<string, List<string>> GetReaderBorrowedTitlesMap();
    List<OverdueItemViewModel> GetAllOverdueLoans();
    DashboardViewModel GetDashboardStatistics();

    // Sổ quỹ tài chính Thư viện UTC
    FinancialSummaryViewModel GetFinancialSummary();
    List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? searchKeyword, string? selectedType);
}

public interface IReportService
{
    ReportViewModel GetReport(string? period, string? scope);
    byte[] GenerateExcelReport(string? period, string? scope);
}
