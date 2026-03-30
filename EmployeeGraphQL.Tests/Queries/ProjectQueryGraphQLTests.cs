using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

public class ProjectQueryGraphQLTests : IClassFixture<TestFactory>
{
    private readonly GraphQLHelper _graphql;
    private readonly HttpClient _client;

    private const string ProjectsQuery = """
        query Projects($options: QueryOptionsInput!) {
            projects(options: $options) {
                totalCount
                page
                pageSize
                items {
                    projectId
                    projectName
                    samparkType
                    karyakarCount
                    familyCount
                    startDate
                    endDate
                    createdBy
                    status
                }
            }
        }
        """;

    public ProjectQueryGraphQLTests(TestFactory factory)
    {
        _client = factory.CreateClient();
        _graphql = new GraphQLHelper(_client);
    }

    private static JsonElement ParseData(string json)
    {
        var root = JsonDocument.Parse(json).RootElement;
        if (!root.TryGetProperty("data", out var data))
            throw new InvalidOperationException($"GraphQL response has no 'data' field. Body: {json}");
        return data;
    }

    private static bool HasErrors(string json)
        => JsonDocument.Parse(json).RootElement.TryGetProperty("errors", out _);

    private static JsonElement ParseErrors(string json)
        => JsonDocument.Parse(json).RootElement.GetProperty("errors");

    // ─────────────────────────────────────────────────────────────
    // Basic Query
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Projects_DefaultOptions_Returns200WithPagedResult()
    {
        var variables = new { options = new { page = 1, pageSize = 20 } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var projects = ParseData(body).GetProperty("projects");
        Assert.True(projects.TryGetProperty("totalCount", out _), "Missing: totalCount");
        Assert.True(projects.TryGetProperty("page", out _), "Missing: page");
        Assert.True(projects.TryGetProperty("pageSize", out _), "Missing: pageSize");
        Assert.True(projects.TryGetProperty("items", out _), "Missing: items");
    }

    [Fact]
    public async Task Projects_ResponseShape_ItemsContainAllFields()
    {
        var variables = new { options = new { page = 1, pageSize = 5 } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var items = ParseData(body).GetProperty("projects").GetProperty("items");

        if (items.GetArrayLength() > 0)
        {
            var first = items[0];
            Assert.True(first.TryGetProperty("projectId", out _), "Missing: projectId");
            Assert.True(first.TryGetProperty("projectName", out _), "Missing: projectName");
            Assert.True(first.TryGetProperty("samparkType", out _), "Missing: samparkType");
            Assert.True(first.TryGetProperty("karyakarCount", out _), "Missing: karyakarCount");
            Assert.True(first.TryGetProperty("familyCount", out _), "Missing: familyCount");
            Assert.True(first.TryGetProperty("startDate", out _), "Missing: startDate");
            Assert.True(first.TryGetProperty("endDate", out _), "Missing: endDate");
            Assert.True(first.TryGetProperty("createdBy", out _), "Missing: createdBy");
            Assert.True(first.TryGetProperty("status", out _), "Missing: status");
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Pagination
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Projects_PageSize5_ReturnsAtMost5Items()
    {
        var variables = new { options = new { page = 1, pageSize = 5 } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var items = ParseData(body).GetProperty("projects").GetProperty("items");
        Assert.True(items.GetArrayLength() <= 5, $"Expected at most 5 items but got {items.GetArrayLength()}");
    }

    [Fact]
    public async Task Projects_PageReturnsCorrectPageNumber()
    {
        var variables = new { options = new { page = 2, pageSize = 5 } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var projects = ParseData(body).GetProperty("projects");
        Assert.Equal(2, projects.GetProperty("page").GetInt32());
        Assert.Equal(5, projects.GetProperty("pageSize").GetInt32());
    }

    // ─────────────────────────────────────────────────────────────
    // Search Filter
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Projects_SearchFilter_NonExistentTerm_ReturnsEmpty()
    {
        var variables = new { options = new { page = 1, pageSize = 20, search = "NONEXISTENT_XYZ_12345" } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var projects = ParseData(body).GetProperty("projects");
        Assert.Equal(0, projects.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, projects.GetProperty("items").GetArrayLength());
    }

    // ─────────────────────────────────────────────────────────────
    // Status Filter
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Projects_StatusFilter_ReturnsOnlyMatchingStatus()
    {
        var variables = new { options = new { page = 1, pageSize = 20, status = "DRAFT" } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.False(HasErrors(body), $"Unexpected errors: {body}");

        var items = ParseData(body).GetProperty("projects").GetProperty("items");
        foreach (var item in items.EnumerateArray())
        {
            Assert.Equal("DRAFT", item.GetProperty("status").GetString());
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Sorting
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Projects_SortByNameAsc_Returns200()
    {
        var variables = new { options = new { page = 1, pageSize = 10, sortBy = "name", sortOrder = "asc" } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");
    }

    [Fact]
    public async Task Projects_SortByStartDate_Returns200()
    {
        var variables = new { options = new { page = 1, pageSize = 10, sortBy = "startdate", sortOrder = "desc" } };

        var response = await _graphql.ExecuteMutation(ProjectsQuery, variables);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasErrors(body), $"Unexpected errors: {body}");
    }

    // ─────────────────────────────────────────────────────────────
    // Auth / Header Validation
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Projects_MissingDepartmentIdHeader_ReturnsError()
    {
        const string inlineQuery = """
            query {
                projects(options: { page: 1, pageSize: 10 }) {
                    totalCount
                    items { projectId }
                }
            }
            """;

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = JsonContent.Create(new { query = inlineQuery });
        request.Headers.Add("Authorization", "Bearer test-token");
        request.Headers.Add("X-App-Position", "51134");
        request.Headers.Add("X-App-Event", "1");

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(HasErrors(body), $"Expected error for missing departmentId but got: {body}");
    }

    [Fact]
    public async Task Projects_MissingAuthHeader_ReturnsAuthError()
    {
        const string inlineQuery = """
            query {
                projects(options: { page: 1, pageSize: 10 }) {
                    totalCount
                    items { projectId }
                }
            }
            """;

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        request.Content = JsonContent.Create(new { query = inlineQuery });

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        var isUnauthorized = response.StatusCode == HttpStatusCode.Unauthorized
            || HasErrors(body);

        Assert.True(isUnauthorized, $"Expected auth failure but got {response.StatusCode}: {body}");
    }
}
