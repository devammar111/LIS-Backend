namespace LIS.Api.Models;

/// <summary>
/// Single source of truth for converting the wire/display strings (e.g. "Lipid Panel")
/// to and from the strongly-typed <see cref="TestType"/> / <see cref="Priority"/> enums.
/// Used by both the validators (string -> enum) and the response mapping (enum -> string)
/// so the frontend contract ("Lipid Panel" with a space) never drifts.
/// </summary>
public static class EnumDisplay
{
    private static readonly IReadOnlyDictionary<TestType, string> TestTypeToDisplay = new Dictionary<TestType, string>
    {
        [TestType.CBC] = "CBC",
        [TestType.BMP] = "BMP",
        [TestType.LipidPanel] = "Lipid Panel",
        [TestType.UA] = "UA"
    };

    private static readonly IReadOnlyDictionary<Priority, string> PriorityToDisplay = new Dictionary<Priority, string>
    {
        [Priority.Routine] = "Routine",
        [Priority.STAT] = "STAT"
    };

    public static string ToDisplay(this TestType testType) => TestTypeToDisplay[testType];

    public static string ToDisplay(this Priority priority) => PriorityToDisplay[priority];

    public static bool TryParseTestType(string? value, out TestType testType)
    {
        testType = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        foreach (var (enumValue, display) in TestTypeToDisplay)
        {
            if (display.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            {
                testType = enumValue;
                return true;
            }
        }

        return false;
    }

    public static bool TryParsePriority(string? value, out Priority priority)
    {
        priority = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        foreach (var (enumValue, display) in PriorityToDisplay)
        {
            if (display.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            {
                priority = enumValue;
                return true;
            }
        }

        return false;
    }
}
