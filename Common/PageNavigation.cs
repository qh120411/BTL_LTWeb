namespace BTL_LTWEB.Common;

public static class PageNavigation
{
    public static IReadOnlyList<int?> GetPages(int current, int total)
    {
        total = Math.Max(1, total);
        current = Math.Clamp(current, 1, total);
        var pages = new SortedSet<int> { 1, total };
        for (var p = Math.Max(1, current - 1); p <= Math.Min(total, current + 1); p++) pages.Add(p);
        if (current <= 2 && total >= 3) pages.Add(3);
        if (current >= total - 1 && total >= 3) pages.Add(total - 2);
        var result = new List<int?>();
        var previous = 0;
        foreach (var p in pages)
        {
            if (p - previous == 2) result.Add(previous + 1);
            else if (p - previous > 2) result.Add(null);
            result.Add(p);
            previous = p;
        }
        return result;
    }
}
