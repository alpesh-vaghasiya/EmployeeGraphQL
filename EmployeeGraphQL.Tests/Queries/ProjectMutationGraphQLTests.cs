using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

public class ProjectMutationGraphQLTests : IClassFixture<TestFactory>
{
    private readonly GraphQLHelper _graphql;
    private readonly HttpClient _client;

    private const string CreateProjectMutation = """
        mutation CreateProject($input: ProjectInput!) {
            createProject(input: $input) {
                projectId
                title
                description
                status
                templateId
                reminderFrequency
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
                projectStartDate
                projectEndDate
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

    public ProjectMutationGraphQLTests(TestFactory factory)
    {
        _client = factory.CreateClient();
        _graphql = new GraphQLHelper(_client);
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    private static JsonElement ParseData(string json)
    {
        var root = JsonDocument.Parse(json).RootElement;
        if (!root.TryGetProperty("data", out var data))
            throw new InvalidOperationException($"GraphQL response has no 'data' field. Body: {json}");
        return data;
    }

    private static JsonElement ParseErrors(string json)
        => JsonDocument.Parse(json).RootElement.GetProperty("errors");

    private static bool HasErrors(string json)
        => JsonDocument.Parse(json).RootElement.TryGetProperty("errors", out _);

    private async Task<long> CreateProjectAndGetId(string name = "Auto Create Project", long templateId = 1)
    {
        var variables = new { input = new { name, templateId } };
        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();
        return ParseData(body).GetProperty("createProject").GetProperty("projectId").GetInt64();
    }

    // ─────────────────────────────────────────────────────────────
    // createProject
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProject_ValidInput_Returns200WithDraftStatus()
    {
        var uniqueTitle = $"GQL-Create-{Guid.NewGuid():N}";
        var variables = new
        {
            input = new
            {
                name = uniqueTitle,
                templateId = 1,
                description = "Created via GraphQL test",
                reminderFrequency = "ONCE"
            }
        };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(HasErrors(body), $"Unexpected GraphQL errors: {body}");

        var project = ParseData(body).GetProperty("createProject");
        Assert.True(project.GetProperty("projectId").GetInt64() > 0);
        Assert.Equal(uniqueTitle, project.GetProperty("title").GetString());
        Assert.Equal("DRAFT", project.GetProperty("status").GetString());
        Assert.Equal(1, project.GetProperty("templateId").GetInt64());
    }

    [Fact]
    public async Task CreateProject_ResponseShape_ContainsAllRequestedFields()
    {
        var variables = new { input = new { name = $"GQL-Shape-{Guid.NewGuid():N}", templateId = 1 } };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();
        var project = ParseData(body).GetProperty("createProject");

        Assert.True(project.TryGetProperty("projectId",        out _), "Missing: projectId");
        Assert.True(project.TryGetProperty("title",            out _), "Missing: title");
        Assert.True(project.TryGetProperty("description",      out _), "Missing: description");
        Assert.True(project.TryGetProperty("status",           out _), "Missing: status");
        Assert.True(project.TryGetProperty("templateId",       out _), "Missing: templateId");
        Assert.True(project.TryGetProperty("projectStartDate", out _), "Missing: projectStartDate");
        Assert.True(project.TryGetProperty("projectEndDate",   out _), "Missing: projectEndDate");
    }

    [Fact]
    public async Task CreateProject_TemplateNotFound_ReturnsGraphQLError()
    {
        var variables = new { input = new { name = "Orphan Project", templateId = 999999 } };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(HasErrors(body), $"Expected errors but got: {body}");
        Assert.Equal("Template not found", ParseErrors(body)[0].GetProperty("message").GetString());
    }

    [Fact]
    public async Task CreateProject_EmptyName_ReturnsValidationError()
    {
        var variables = new { input = new { name = "", templateId = 1 } };

        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(HasErrors(body), $"Expected validation error but got: {body}");
    }

    [Fact]
    public async Task CreateProject_DuplicateTitle_ReturnsDuplicateTitleError()
    {
        var title = $"Duplicate-{Guid.NewGuid():N}";
        var variables = new { input = new { name = title, templateId = 1 } };

        // First create succeeds
        await _graphql.ExecuteMutation(CreateProjectMutation, variables);

        // Second create with same title fails
        var response = await _graphql.ExecuteMutation(CreateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(HasErrors(body), $"Expected duplicate error but got: {body}");
        Assert.Equal("Project title already exists.", ParseErrors(body)[0].GetProperty("message").GetString());
    }

    [Fact]
    public async Task CreateProject_MissingAuthHeader_ReturnsAuthError()
    {
        // Send inline query string without variable types to avoid schema-validation BadRequest
        const string inlineQuery = """
            mutation {
                createProject(input: { name: "Unauth Project", templateId: 1 }) {
                    projectId
                    status
                }
            }
            """;

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = JsonContent.Create(new { query = inlineQuery });
        // No Authorization header

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        var isUnauthorized = response.StatusCode == HttpStatusCode.Unauthorized
            || (response.StatusCode == HttpStatusCode.OK && HasErrors(body));

        Assert.True(isUnauthorized, $"Expected auth failure but got {response.StatusCode}: {body}");
    }

    // ─────────────────────────────────────────────────────────────
    // updateProject
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProject_ExistingProject_ReturnsUpdatedFields()
    {
        var projectId = await CreateProjectAndGetId($"To-Be-Updated-{Guid.NewGuid():N}");
        var updatedTitle = $"GQL-Updated-{Guid.NewGuid():N}";

        var variables = new
        {
            id = projectId,
            input = new
            {
                name = updatedTitle,
                templateId = 1,
                description = "Updated description"
            }
        };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var project = ParseData(body).GetProperty("updateProject");
        Assert.Equal(updatedTitle, project.GetProperty("title").GetString());
        Assert.Equal("Updated description", project.GetProperty("description").GetString());
        Assert.Equal(projectId, project.GetProperty("projectId").GetInt64());
    }

    [Fact]
    public async Task UpdateProject_IdFieldPassedCorrectly_ReturnsSameProjectId()
    {
        var projectId = await CreateProjectAndGetId($"Id-Check-{Guid.NewGuid():N}");

        var updateTitle = $"GQL-Id-{Guid.NewGuid():N}";
        var variables = new
        {
            id = projectId,
            input = new { name = updateTitle, templateId = 1 }
        };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();

        var returnedId = ParseData(body).GetProperty("updateProject").GetProperty("projectId").GetInt64();
        Assert.Equal(projectId, returnedId);
    }

    [Fact]
    public async Task UpdateProject_NonExistingId_ReturnsNotFoundError()
    {
        var variables = new
        {
            id = 999999L,
            input = new { name = "Ghost Project", templateId = 1 }
        };

        var response = await _graphql.ExecuteMutation(UpdateProjectMutation, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(HasErrors(body));
        Assert.Equal("Project not found", ParseErrors(body)[0].GetProperty("message").GetString());
    }

    // ─────────────────────────────────────────────────────────────
    // deleteProject
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProject_ExistingProject_ReturnsTrue()
    {
        var projectId = await CreateProjectAndGetId($"To-Be-Deleted-{Guid.NewGuid():N}");

        var response = await _graphql.ExecuteMutation(DeleteProjectMutation, new { id = projectId });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");
        Assert.True(ParseData(body).GetProperty("deleteProject").GetBoolean());
    }

    [Fact]
    public async Task DeleteProject_AlreadyDeleted_ReturnsNotFoundError()
    {
        var projectId = await CreateProjectAndGetId($"Delete-Twice-{Guid.NewGuid():N}");

        // First delete
        await _graphql.ExecuteMutation(DeleteProjectMutation, new { id = projectId });

        // Second delete — should fail
        var response = await _graphql.ExecuteMutation(DeleteProjectMutation, new { id = projectId });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(HasErrors(body), $"Expected not-found error on second delete but got: {body}");
    }

    [Fact]
    public async Task DeleteProject_NonExistingId_ReturnsNotFoundError()
    {
        var response = await _graphql.ExecuteMutation(DeleteProjectMutation, new { id = 999999L });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(HasErrors(body));
        Assert.Equal("Project not found", ParseErrors(body)[0].GetProperty("message").GetString());
    }

    // ─────────────────────────────────────────────────────────────
    // publishProject
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishProject_DraftProject_ReturnsPublishedStatus()
    {
        var projectId = await CreateProjectAndGetId($"To-Be-Published-{Guid.NewGuid():N}");

        var response = await _graphql.ExecuteMutation(PublishProjectMutation, new { id = projectId });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var project = ParseData(body).GetProperty("publishProject");
        Assert.Equal("PUBLISH", project.GetProperty("status").GetString());
        Assert.Equal(projectId, project.GetProperty("projectId").GetInt64());
    }

    [Fact]
    public async Task PublishProject_NonExistingId_ReturnsNotFoundError()
    {
        var response = await _graphql.ExecuteMutation(PublishProjectMutation, new { id = 999999L });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(HasErrors(body));
        Assert.Equal("Project not found", ParseErrors(body)[0].GetProperty("message").GetString());
    }
}
