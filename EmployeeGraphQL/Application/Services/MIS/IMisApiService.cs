public interface IMisApiService
{
    Task<T> GetAsync<T>(string endpoint, Dictionary<string, object> queryParams = null);
    Task<bool> ValidateMisId(string misId);
}