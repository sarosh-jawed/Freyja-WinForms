using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace Freyja.Csv.CsvServices;

public sealed class CsvLoader
{
    private static CsvConfiguration CreateConfig() =>
        new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,

            // This skips rows that are effectively empty (all fields null/whitespace)
            ShouldSkipRecord = args =>
            {
                var record = args.Row?.Parser?.Record;
                if (record == null) return false;

                // skip if every column is empty/whitespace
                return record.All(field => string.IsNullOrWhiteSpace(field));
            },

            // Reliability settings: so that we don’t throw on minor format issues
            MissingFieldFound = null,
            HeaderValidated = null,

            // Optional: To log or ignore bad data
            BadDataFound = null
        };

    public List<T> Load<T, TMap>(string path)
        where TMap : CsvHelper.Configuration.ClassMap<T>
    {
        using var stream = new StreamReader(
            path,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            detectEncodingFromByteOrderMarks: true);

        using var csv = new CsvReader(stream, CreateConfig());
        csv.Context.RegisterClassMap<TMap>();

        return csv.GetRecords<T>().ToList();
    }

    public string[] ReadHeaders(string path)
    {
        using var stream = new StreamReader(
            path,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            detectEncodingFromByteOrderMarks: true);

        using var csv = new CsvReader(stream, CreateConfig());

        csv.Read();
        csv.ReadHeader();
        return csv.HeaderRecord ?? Array.Empty<string>();
    }
}
