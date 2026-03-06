public record TemplateDocumentInput(
    string DocumentName,
    string? DocumentUrl,
    string? DocumentSfsId,
    string? DocumentType,
    long? FileSize,
    bool IsOptional
);