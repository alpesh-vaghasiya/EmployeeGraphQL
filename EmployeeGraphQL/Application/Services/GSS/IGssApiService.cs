public interface IGssService
{
    Task<T> GetAsync<T>(string endpoint, Dictionary<string, object> queryParams = null);
}