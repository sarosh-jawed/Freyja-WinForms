namespace Freyja.Csv.CsvServices;

public static class CsvHeaderValidator
{
    public static List<string> MissingHeaders(string[] actualHeaders, params string[] required)
    {
        var actualSet = new HashSet<string>(
            actualHeaders
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim()),
            StringComparer.OrdinalIgnoreCase);

        return required
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Where(r => !actualSet.Contains(r))
            .ToList();
    }
}
