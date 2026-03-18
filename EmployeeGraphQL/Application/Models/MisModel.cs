public class MisModel
{
    public string Url { get; set; }
    public string AppId { get; set; }
    public string AppSecret { get; set; }
}

public class MisApiResponse<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}