namespace Api.GraphQL.Inputs;

public class ProjectInput
{
    public string Name { get; set; } = null!;
    public long TemplateId { get; set; }
    public string? Description { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public string? ReminderFrequency { get; set; }
    public ReminderConfigInput? ReminderFrequencyConfig { get; set; }
}