using EmployeeGraphQL.GraphQL.Inputs;

public interface ISFSService
  {
    Task<UploadRequest> UploadURL(UploadRequestModel FileId);
    Task<UploadResponse> DownloadURL(Guid FileId);
  }