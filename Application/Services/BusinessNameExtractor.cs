using System.Text.RegularExpressions;

public static class BusinessNameExtractor
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "transfer", "payment", "purchase", "debit", "credit"
    };

    public static string ExtractBusinessName(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return description;

        string cleaned = description.Trim();

        // Remove trailing noise
        string[] patterns = {
            @" - Receipt \d+.*",
            @" - Direct Debit.*",
            @"\s+\d{2}/\d{2}.*",
            @"\s+\*\d+.*",
            @"\s+#\d+.*",
            @"\s+REF:.*",
            @"\s+AUTH:.*",
            @"\s+PURCHASE.*",
            @"\s+PAYMENT.*"
        };

        foreach (var pattern in patterns)
            cleaned = Regex.Replace(cleaned, pattern, "", RegexOptions.IgnoreCase);

        // Take up to the first dash
        var dashIndex = cleaned.IndexOf(" - ");
        if (dashIndex > 0)
            cleaned = cleaned.Substring(0, dashIndex);

        cleaned = cleaned.Trim();

        if (StopWords.Contains(cleaned))
            return string.Empty;

        return cleaned;
    }
}
