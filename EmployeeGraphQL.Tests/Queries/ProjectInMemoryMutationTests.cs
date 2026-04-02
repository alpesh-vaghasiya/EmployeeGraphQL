using System.Net;
using System.Text.Json;
using EmployeeGraphQL.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Integration tests for Project mutations running entirely on InMemory database.
/// No PostgreSQL connection required.
/// </summary>
public class ProjectInMemoryMutationTests : IClassFixture<TestFactory>
{
    private readonly TestFactory    _factory;
    private readonly GraphQLHelper  _graphql;

    private const string CreateProjectMutation = """
        mutation CreateProject($input: ProjectInput!) {
            createProject(input: $input) {
                projectId
                title
                description
                status
                templateId
                projectStartDate
                projectEndDate
            }
        }
        """;

    private const string UpdateProjectMutation = """
        mutation UpdateProject($id: Long!, $input: ProjectInput!) {
            updateProject(id: $id, input: $input) {
                projectId
                title
                description
                status
            }
        }
        """;

    private const string DeleteProjectMutation = """
        mutation DeleteProject($id: Long!) {
            deleteProject(id: $id)
        }
        """;

    private const string PublishProjectMutation = """
        mutation PublishProject($id: Long!) {
            publishProject(id: $id) {
                projectId
                status
            }
        }
        """;

    public ProjectInMemoryMutationTests(TestFactory factory)
    {
        _factory = factory;
        _graphql  = new GraphQLHelper(factory.CreateClient());
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static JsonElement ParseData(string json)
    {
        var root = JsonDocument.Parse(json).RootElement;
        if (!root.TryGetProperty("data", out var data))
            throw new InvalidOperationException($"GraphQL response missing 'data'. Body: {json}");
        return data;
    }

    private static bool HasErrors(string json)
        => JsonDocument.Parse(json).RootElement.TryGetProperty("errors", out _);

    private static string FirstErrorMessage(string json)
        => JsonDocument.Parse(json).RootElement
               .GetProperty("errors")[0]
               .GetProperty("message")
               .GetString()!;

    private async Task<AppDbContext> GetDbAsync()
    {
        // Create a scope that lives as long as the caller holds the reference.
        // Callers must dispose the scope themselves, or just use it inline.
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    private async Task EnsureTemplateAsync(long templateId = 1, int daysUntilStart = -30, int daysUntilEnd = 60)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDbSeeder.EnsureTemplateAsync(db, templateId, daysUntilStart, daysUntilEnd);
    }

    private async Task<long> SeedProjectAsync(long templateId = 1, string? title = null, string status = "DRAFT")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var project = await TestDbSeeder.SeedProjectAsync(db, templateId, title, status);
        return project.ProjectId;
    }

    // ── CreateProject – happy path ────────────────────────────────────────

    [Fact]
    public async Task CreateProject_ValidInput_Returns200WithDraftProject()
    {
        await EnsureTemplateAsync(templateId: 1);

        var title     = $"New-{Guid.NewGuid():N}";
        var variables = new { input = new { name = title, templateId = 1, description = "Test" } };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var data = ParseData(body).GetProperty("createProject");
        Assert.Equal(title,    data.GetProperty("title").GetString());
        Assert.Equal("DRAFT",  data.GetProperty("status").GetString());
        Assert.Equal(1L,       data.GetProperty("templateId").GetInt64());
        Assert.True(data.GetProperty("projectId").GetInt64() > 0);
    }

    [Fact]
    public async Task CreateProject_SetsTemplateDates_WhenNoDatesSupplied()
    {
        await EnsureTemplateAsync(templateId: 2, daysUntilStart: 5, daysUntilEnd: 30);

        var variables = new { input = new { name = $"AutoDate-{Guid.NewGuid():N}", templateId = 2 } };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var data  = ParseData(body).GetProperty("createProject");
        var start = data.GetProperty("projectStartDate").GetDateTime();
        var end   = data.GetProperty("projectEndDate").GetDateTime();

        Assert.True(start < end, "Expected start date before end date");
    }

    // ── CreateProject – error cases ───────────────────────────────────────

    [Fact]
    public async Task CreateProject_TemplateNotFound_ReturnsError()
    {
        var variables = new { input = new { name = $"Ghost-{Guid.NewGuid():N}", templateId = 99999 } };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Template not found", FirstErrorMessage(body));
    }

    [Fact]
    public async Task CreateProject_DuplicateTitle_SameLocation_ReturnsError()
    {
        await EnsureTemplateAsync(templateId: 1);

        var title     = $"Dup-{Guid.NewGuid():N}";
        var variables = new { input = new { name = title, templateId = 1 } };

        await _graphql.ExecuteMutation(CreateProjectMutation, variables);        // first – succeeds
        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables); // second – duplicate
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Project title already exists.", FirstErrorMessage(body));
    }

    // ── UpdateProject – happy path ────────────────────────────────────────

    [Fact]
    public async Task UpdateProject_ValidInput_UpdatesTitle()
    {
        await EnsureTemplateAsync(templateId: 1);
        var projectId = await SeedProjectAsync(templateId: 1);

        var newTitle  = $"Updated-{Guid.NewGuid():N}";
        var variables = new { id = projectId, input = new { name = newTitle, templateId = 1 } };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var data = ParseData(body).GetProperty("updateProject");
        Assert.Equal(newTitle,   data.GetProperty("title").GetString());
        Assert.Equal(projectId,  data.GetProperty("projectId").GetInt64());
    }

    [Fact]
    public async Task UpdateProject_UpdatesDescription()
    {
        await EnsureTemplateAsync(templateId: 1);
        var projectId = await SeedProjectAsync(templateId: 1);

        var variables = new
        {
            id    = projectId,
            input = new { name = $"Title-{Guid.NewGuid():N}", templateId = 1, description = "New desc" }
        };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");
        Assert.Equal("New desc", ParseData(body).GetProperty("updateProject")
                                                .GetProperty("description").GetString());
    }

    // ── UpdateProject – error cases ───────────────────────────────────────

    [Fact]
    public async Task UpdateProject_NotFound_ReturnsError()
    {
        var variables = new { id = 999999L, input = new { name = $"X-{Guid.NewGuid():N}", templateId = 1 } };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Project not found", FirstErrorMessage(body));
    }

    [Fact]
    public async Task UpdateProject_DuplicateTitle_ReturnsError()
    {
        await EnsureTemplateAsync(templateId: 1);

        var existingTitle = $"Existing-{Guid.NewGuid():N}";
        await SeedProjectAsync(templateId: 1, title: existingTitle);
        var toUpdateId = await SeedProjectAsync(templateId: 1);   // second project

        var variables = new { id = toUpdateId, input = new { name = existingTitle, templateId = 1 } };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Project title already exists.", FirstErrorMessage(body));
    }

    // ── DeleteProject ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProject_ExistingProject_ReturnsTrue()
    {
        await EnsureTemplateAsync(templateId: 1);
        var projectId = await SeedProjectAsync(templateId: 1);

        var response = await _graphql.ExecuteMutation(DeleteProjectMutation, new { id = projectId });
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");
        Assert.True(ParseData(body).GetProperty("deleteProject").GetBoolean());
    }

    [Fact]
    public async Task DeleteProject_NotFound_ReturnsError()
    {
        var response = await _graphql.ExecuteMutation(DeleteProjectMutation, new { id = 999999L });
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Project not found", FirstErrorMessage(body));
    }

    // ── PublishProject ────────────────────────────────────────────────────

    [Fact]
    public async Task PublishProject_DraftProject_SetsStatusToPublish()
    {
        await EnsureTemplateAsync(templateId: 1);
        var projectId = await SeedProjectAsync(templateId: 1, status: "DRAFT");

        var response = await _graphql.ExecuteMutation(PublishProjectMutation, new { id = projectId });
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var data = ParseData(body).GetProperty("publishProject");
        Assert.Equal("PUBLISH", data.GetProperty("status").GetString());
        Assert.Equal(projectId, data.GetProperty("projectId").GetInt64());
    }

    [Fact]
    public async Task PublishProject_AlreadyPublished_ReturnsError()
    {
        await EnsureTemplateAsync(templateId: 1);
        var projectId = await SeedProjectAsync(templateId: 1, status: "PUBLISH");

        var response = await _graphql.ExecuteMutation(PublishProjectMutation, new { id = projectId });
        var body     = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Project already published", FirstErrorMessage(body));
    }
}
