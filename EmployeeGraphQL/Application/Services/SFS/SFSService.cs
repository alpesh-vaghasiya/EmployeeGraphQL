
using EmployeeGraphQL.GraphQL.Inputs;
using HttpMultipartParser;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EmployeeGraphQL.Application.Services.SFS
{
    public class SFSService : ISFSService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOptions<SFSServiceModel> SFSoptions;
        public SFSService(
        IHttpClientFactory httpClientFactory,
        IOptions<SFSServiceModel> sFSoptions
        )
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            SFSoptions = sFSoptions;
        }

        public async Task<UploadRequest> UploadURL(UploadRequestModel FileId)
        {
            if (SFSoptions == null || string.IsNullOrEmpty(SFSoptions.Value.AuthId) || string.IsNullOrEmpty(SFSoptions.Value.AuthSecret) || string.IsNullOrEmpty(SFSoptions.Value.Bucket))
            {
                throw new Exception("SFS Service configuration required.");
            }

            var createApiModel = new UploadRequest();
            string endPoint = "b/{0}/bulk_upload";
            endPoint = string.Concat(SFSoptions.Value.Url, string.Format(endPoint, SFSoptions.Value.Bucket));


            var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Put, endPoint);
            request.Headers.Add("x-app-auth-id", SFSoptions.Value.AuthId);
            request.Headers.Add("x-app-auth-secret", SFSoptions.Value.AuthSecret);
            request.Headers.Add("x-app-api-version", SFSoptions.Value.ApiVersion);
            request.Headers.Add("User-Agent", "EmployeeGraphQLClient/1.0");
            var content = new StringContent(JsonConvert.SerializeObject(FileId), null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            createApiModel = JsonConvert.DeserializeObject<UploadRequest>(await response.Content.ReadAsStringAsync());
            createApiModel.StatusCode = response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                foreach (var item in createApiModel.Data)
                {
                    item.PreviewUrl = string.Concat(SFSoptions.Value.PreviewBaseUrl, item.Key);
                }
            }

            return createApiModel;
        }
        public async Task<UploadResponse> DownloadURL(Guid FileId)
        {
            if (SFSoptions == null || string.IsNullOrEmpty(SFSoptions.Value.AuthId) || string.IsNullOrEmpty(SFSoptions.Value.AuthSecret) || string.IsNullOrEmpty(SFSoptions.Value.Bucket))
            {
                throw new Exception("SFS Service configuration required.");
            }

            var getApiModel = new UploadResponse();
            try
            {
                string endPoint = "b/{0}/f/";
                endPoint = string.Concat(SFSoptions.Value.Url, string.Format(endPoint, SFSoptions.Value.Bucket));

                var client = httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, string.Concat(endPoint, FileId.ToString()));
                request.Headers.Add("x-app-auth-id", SFSoptions.Value.AuthId);
                request.Headers.Add("x-app-auth-secret", SFSoptions.Value.AuthSecret);
                request.Headers.Add("x-app-api-version", SFSoptions.Value.ApiVersion);
                request.Headers.Add("User-Agent", "EmployeeGraphQLClient/1.0");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var parser = MultipartFormDataParser.Parse(response.Content.ReadAsStream());

                var file = parser.Files[0];
                getApiModel.FileName = file.FileName;
                string ContentType = file.ContentType;
                if (!string.IsNullOrEmpty(getApiModel?.FileName))
                {
                    if (getApiModel.FileName.ToLower().EndsWith(".html"))
                    {
                        ContentType = "text/html";
                    }
                    else if (getApiModel.FileName.ToLower().EndsWith(".htm"))
                    {
                        ContentType = "text/htm";
                    }
                }
                getApiModel.ContentType = ContentType;
                getApiModel.data = file.Data;

                return getApiModel;
            }
            catch (Exception)
            {
                return getApiModel;
            }
        }
    }
}
