using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public class CsvService
{
    public List<DepartmentCsvDto> ReadDepartmentCsv(TextReader textReader)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };
        using var csv = new CsvReader(textReader, config);
        return csv.GetRecords<DepartmentCsvDto>().ToList();
    }
}