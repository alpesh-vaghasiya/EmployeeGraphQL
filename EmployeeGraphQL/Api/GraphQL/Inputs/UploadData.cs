using System.Net;

namespace EmployeeGraphQL.GraphQL.Inputs
{
    public class SFSServiceModel
    {
        private string url;
        private string PreviewendPoint = "FileManage/";
        private string previewBaseUrl;

        public string PreviewBaseUrl { get => string.Concat(previewBaseUrl, PreviewendPoint); set => previewBaseUrl = value; }
        public string Url { get => url; set => url = value; }
        public string Bucket { get; set; }
        public string AuthId { get; set; }
        public string AuthSecret { get; set; }
        public string ApiVersion { get; set; }
    }
    public class UploadRequestModel
    {
        public List<string> keys { get; set; } = new List<string>();
    }
    public class UploadData
    {
        public string Key { get; set; }
        public string UploadUrl { get; set; }
        public string PreviewUrl { get; set; }
        public string FileName { get; set; }
    }
    public class UploadRequest
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<UploadData> Data { get; set; }
    }

    public class UploadResponse
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public Stream data { get; set; }
    }
}
