using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;

/// <summary>
/// Utility for seeding InMemory AppDbContext with common test entities.
/// All methods are idempotent – safe to call from multiple tests sharing
/// the same IClassFixture&lt;TestFactory&gt;.
/// </summary>
public static class TestDbSeeder
{
    /// <summary>
    /// Ensures a template with the given ID exists. Creates it only if absent.
    /// </summary>
    public static async Task<Template> EnsureTemplateAsync(
        AppDbContext db,
        long templateId = 1,
        int daysUntilStart = -30,
        int daysUntilEnd = 60)
    {
        var existing = await db.Templates.FindAsync(templateId);
        if (existing != null)
            return existing;

        var template = new Template
        {
            TemplateId           = templateId,
            TemplateUucode       = Guid.NewGuid(),
            Title                = $"Test Template {templateId}",
            AllowedDraftProject  = "[]",
            ProjectTypeId        = 1,
            SamparkTypeId        = 1,
            StartDate            = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(daysUntilStart)),
            EndDate              = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(daysUntilEnd)),
            ReminderFrequencyConfig = null,
        };

        db.Templates.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    /// <summary>
    /// Seeds a project with a unique title. Does NOT check for duplicates –
    /// pass an explicit title only when you need a known value.
    /// </summary>
    public static async Task<Project> SeedProjectAsync(
        AppDbContext db,
        long templateId = 1,
        string? title = null,
        string status = "DRAFT")
    {
        var project = new Project
        {
            ProjectUucode    = Guid.NewGuid(),
            TemplateId       = templateId,
            Title            = title ?? $"Seeded-{Guid.NewGuid():N}",
            Description      = "Seeded for tests",
            Status           = status,
            ProjectStartDate = DateTime.UtcNow.Date,
            ProjectEndDate   = DateTime.UtcNow.Date.AddDays(30),
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    /// <summary>
    /// Convenience: seeds both a template and a project in one call.
    /// </summary>
    public static async Task<(Template Template, Project Project)> SeedTemplateWithProjectAsync(
        AppDbContext db,
        long templateId = 1,
        string? projectTitle = null,
        string projectStatus = "DRAFT")
    {
        var template = await EnsureTemplateAsync(db, templateId);
        var project  = await SeedProjectAsync(db, templateId, projectTitle, projectStatus);
        return (template, project);
    }
}
