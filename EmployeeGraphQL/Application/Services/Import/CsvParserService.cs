using System.Net.Http;

public class CsvParserService
{
    private readonly HttpClient _httpClient;

    public CsvParserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SyncKaryakarCsvRow>> ParseAsync(string fileUrl)
    {
        var csvContent = await _httpClient.GetStringAsync(fileUrl);

        var lines = csvContent.Split('\n');

        var rows = new List<SyncKaryakarCsvRow>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var columns = lines[i].Split(',');

            rows.Add(new SyncKaryakarCsvRow
            {
                RowNumber = i,
                MisId = columns[0]
            });
        }

        return rows;
    }
}