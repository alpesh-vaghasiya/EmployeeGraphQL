using EmployeeGraphQL.GraphQL.Inputs;
public interface IFileService
{
    Task<List<UploadData>> UploadUrls(string[] fileNames);
    Task<DownloadFileType> GetFile(Guid fileId);
}