public class TemplateDocumentInput
{
    public string DocumentName { get; set; } = null!;

    public string? DocumentUrl { get; set; }

    public string? DocumentSfsId { get; set; }

    public string? DocumentType { get; set; }

    public long? FileSize { get; set; }

    public bool IsOptional { get; set; }
}