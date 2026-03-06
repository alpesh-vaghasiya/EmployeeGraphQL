namespace EmployeeGraphQL.Domain.Entities
{
    public class TemplateListView
    {
        public long TemplateId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public long ProjectType { get; set; }
        public long SamparkType { get; set; }
        public string? LocationScope { get; set; }

        public string[] Departments { get; set; } = Array.Empty<string>();

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}