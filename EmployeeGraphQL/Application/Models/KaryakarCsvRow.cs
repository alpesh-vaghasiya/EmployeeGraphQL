using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;

public class KaryakarCsvRow
{
    public string MisBapsId { get; set; }
}
// public class RowValidationResult
// {
//     public bool IsValid { get; set; }
//     public string? Error { get; set; }

//     public static RowValidationResult Ok() => new() { IsValid = true };
//     public static RowValidationResult Fail(string error) => new() { IsValid = false, Error = error };
// }