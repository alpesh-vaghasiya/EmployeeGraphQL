public interface IAsmApiService
{
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest body);
}