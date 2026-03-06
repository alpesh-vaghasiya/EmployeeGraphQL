public interface IMisApiService
{
    Task<T> GetAsync<T>(string endpoint, Dictionary<string, object> queryParams = null);
}