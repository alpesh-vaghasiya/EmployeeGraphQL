using EmployeeGraphQL.GraphQL.Inputs;

namespace Api.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class FileMutation
    {
        public async Task<List<UploadData>> UploadUrls(string[] fileNames, [Service] IFileService fileService)
        {
            return await fileService.UploadUrls(fileNames);
        }
    }
}
