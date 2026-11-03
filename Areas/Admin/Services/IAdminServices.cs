using BTL_LTWEB.Areas.Admin.ViewModels;

namespace BTL_LTWEB.Areas.Admin.Services;

public interface IReaderService
{
    ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup);
    ReaderDetailViewModel? GetReaderById(string id);
    bool UpdateReaderSettings(ReaderDetailViewModel model);
    ImportQldtViewModel GetImportPreview();
    bool ProcessImport(out int updatedCount, out int insertedCount);
}

public interface IReportService
{
    ReportViewModel GetReport(string? period, string? scope);
    byte[] GenerateExcelReport(string? period, string? scope);
}

