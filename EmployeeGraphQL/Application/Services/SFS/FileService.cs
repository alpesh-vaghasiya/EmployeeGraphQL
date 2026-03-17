using EmployeeGraphQL.GraphQL.Inputs;

public class FileService : IFileService
{
    private readonly ISFSService _sfsService;

    public FileService(ISFSService sfsService)
    {
        _sfsService = sfsService;
    }

    public async Task<List<UploadData>> UploadUrls(string[] fileNames)
    {
        if (fileNames == null || fileNames.Length == 0)
            throw new Exception("fileNames required");

        var uploadRequest = new UploadRequestModel();

        foreach (var _ in fileNames)
        {
            uploadRequest.keys.Add(Guid.NewGuid().ToString());
        }

        var response = await _sfsService.UploadURL(uploadRequest);

        var result = new List<UploadData>();

        if (response?.Data != null)
        {
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (response.Data.Count > i)
                {
                    var item = response.Data[i];

                    result.Add(new UploadData
                    {
                        Key = item.Key,
                        UploadUrl = item.UploadUrl,
                        PreviewUrl = item.PreviewUrl,
                        FileName = fileNames[i]
                    });
                }
            }
        }

        return result;
    }

    public async Task<DownloadFileType> GetFile(Guid fileId)
    {
        var response = await _sfsService.DownloadURL(fileId);

        if (response?.data == null)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File not found").SetCode("NOT_FOUND").Build());

        using var ms = new MemoryStream();
        await response.data.CopyToAsync(ms);

        return new DownloadFileType
        {
            FileName = response.FileName,
            ContentType = response.ContentType,
            Base64Data = Convert.ToBase64String(ms.ToArray())
        };
    }
}