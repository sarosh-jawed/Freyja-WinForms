namespace Freyja.Csv.CsvServices;

public static class CsvHeaderValidator
{
    public static List<string> MissingHeaders(string[] actualHeaders, params string[] required)
    {
        var set = new HashSet<string>(actualHeaders);
        return required.Where(r => !set.Contains(r)).ToList();
    }
}
