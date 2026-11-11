namespace BTL_LTWEB.Common;

public static class AccountStatus
{
    public static bool IsActive(string? status)
    {
        // Some imported rows stored UTF-8 Vietnamese text after a Windows-1252 decode.
        // Accept only the known active value; never interpret an arbitrary status as active.
        return string.Equals(status?.Trim(), "Hoạt động", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status?.Trim(), "Hoáº¡t Ä‘á»™ng", StringComparison.OrdinalIgnoreCase);
    }
}
