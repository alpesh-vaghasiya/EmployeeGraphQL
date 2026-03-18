public class GssResponse<T>
{
    public bool Succeeded { get; set; }
    public List<T> Data { get; set; }
    public string Message { get; set; }
    public object Errors { get; set; }
}
public class SurveyResponse
{
    public int SurveyId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }
}