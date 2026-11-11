namespace BTL_LTWEB.Common;

public static class LoanTermPolicy
{
    public const string BorrowingWindowMessage = "Chỉ cho mượn trong học kỳ: 11/08–31/12 hoặc 02/01–31/05.";

    public static bool TryGetDueDate(DateOnly borrowedOn, out DateOnly dueDate)
    {
        var semesterTwoStart = new DateOnly(borrowedOn.Year, 1, 2);
        var semesterTwoEnd = new DateOnly(borrowedOn.Year, 5, 31);
        var semesterOneStart = new DateOnly(borrowedOn.Year, 8, 11);
        var semesterOneEnd = new DateOnly(borrowedOn.Year, 12, 31);

        if (borrowedOn >= semesterTwoStart && borrowedOn <= semesterTwoEnd)
        {
            dueDate = semesterTwoEnd;
            return true;
        }
        if (borrowedOn >= semesterOneStart && borrowedOn <= semesterOneEnd)
        {
            dueDate = semesterOneEnd;
            return true;
        }
        dueDate = default;
        return false;
    }

    public static DateOnly GetDueDate(DateOnly borrowedOn) => TryGetDueDate(borrowedOn, out var dueDate)
        ? dueDate
        : throw new ArgumentOutOfRangeException(nameof(borrowedOn), BorrowingWindowMessage);
}
