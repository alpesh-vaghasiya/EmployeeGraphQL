

using Api.GraphQL;
using Api.GraphQL.Auth;
using EmployeeGraphQL.Infrastructure.Data;

[ExtendObjectType(typeof(Query))]
public class FileQuery
{
    [GraphQLName("file")]
    [ModuleAuthorize(new[] { SystemModuleCode.EventGroup }, new[] { ModuleAction.View })]
    public async Task<DownloadFileType> GetFile(
        Guid fileId,
        [Service] IFileService fileService)
    {
        return await fileService.GetFile(fileId);
    }
}
