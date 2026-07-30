using System.Text.RegularExpressions;

namespace Dsw2026Tpi.CrossCutting.Helpers;

public static class ValidationsExtensions
{
    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$";
    public static bool IsEmailValid(this string? email)
    {
        return !string.IsNullOrWhiteSpace(email) &&
            Regex.IsMatch(email, EmailPattern);
    }
    public static bool IsValidTimeRange(this string startTime, string endTime)
    {
        return TimeOnly.TryParse(startTime, out var start) &&
               TimeOnly.TryParse(endTime, out var end) &&
               start < end;
    }
}
