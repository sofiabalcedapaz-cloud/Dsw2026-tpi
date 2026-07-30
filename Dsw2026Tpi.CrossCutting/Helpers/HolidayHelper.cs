using Dsw2026Tpi.CrossCutting.Models;
using System.Text.Json;

namespace Dsw2026Tpi.CrossCutting.Helpers;

public static class HolidayHelper
{
    private static HashSet<DateOnly>? _holidays;

    public static HashSet<DateOnly> Load(string filePath)
    {
        if (_holidays != null) return _holidays;

        var json = File.ReadAllText(filePath);
        var list = JsonSerializer.Deserialize<List<Holiday>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _holidays = list?.Select(h => DateOnly.Parse(h.Fecha)).ToHashSet() ?? [];
        return _holidays;
    }

    public static bool IsHoliday(this DateOnly date, string filePath) =>
        Load(filePath).Contains(date);
}
