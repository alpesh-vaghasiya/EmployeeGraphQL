using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using EmployeeGraphQL.Infrastructure.Data;
using EmployeeGraphQL.Domain.Entities;
using HotChocolate.Authorization;

namespace Api.GraphQL;

[Authorize]
public class Query
{
    // Employees
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Employee> GetEmployees(
        [Service] AppDbContext context)
        => context.Employees;

    [UseProjection]
    public IQueryable<Employee> GetEmployeeById(
        int id,
        [Service] AppDbContext context)
        => context.Employees.Where(e => e.Id == id);


    // Departments
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Department> GetDepartments(
        [Service] AppDbContext context)
        => context.Departments;

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Department> GetDepartmentsNoTracking(
        [Service] AppDbContext context)
        => context.Departments.AsNoTracking();

    [UseProjection]
    public IQueryable<Department> GetDepartmentById(
        int id,
        [Service] AppDbContext context)
        => context.Departments.Where(d => d.Id == id);


    // Projects
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [AllowAnonymous]
    public IQueryable<Project> GetProjects(
        [Service] AppDbContext context)
        => context.Projects;

    [UseProjection]
    public IQueryable<Project> GetProjectById(
        int id,
        [Service] AppDbContext context)
        => context.Projects.Where(p => p.ProjectId == id);

    public async Task<IEnumerable<PersonSearch>> GetPersonSearch(
    PersonSearchInput input,
    [Service] IHttpClientFactory factory,
    [Service] IConfiguration config)
    {
        var client = factory.CreateClient();

        var baseUrl = config["MisApi:Url"];
        var endpoint = "Person/Search";

        var queryParams = new List<string>();

        if (input.PersonSearchID.HasValue)
            queryParams.Add($"personSearchID={input.PersonSearchID.Value}");

        if (input.FamilyID.HasValue)
            queryParams.Add($"familyID={input.FamilyID.Value}");

        if (!string.IsNullOrWhiteSpace(input.FirstName))
            queryParams.Add($"firstName={Uri.EscapeDataString(input.FirstName)}");

        if (!string.IsNullOrWhiteSpace(input.LastName))
            queryParams.Add($"lastName={Uri.EscapeDataString(input.LastName)}");

        if (!string.IsNullOrWhiteSpace(input.Phone))
            queryParams.Add($"phone={Uri.EscapeDataString(input.Phone)}");

        if (!string.IsNullOrWhiteSpace(input.Email))
            queryParams.Add($"email={Uri.EscapeDataString(input.Email)}");

        if (!string.IsNullOrWhiteSpace(input.Address))
            queryParams.Add($"address={Uri.EscapeDataString(input.Address)}");

        if (!string.IsNullOrWhiteSpace(input.City))
            queryParams.Add($"city={Uri.EscapeDataString(input.City)}");

        if (!string.IsNullOrWhiteSpace(input.PostalCode))
            queryParams.Add($"postalCode={Uri.EscapeDataString(input.PostalCode)}");

        if (!string.IsNullOrWhiteSpace(input.CenterName))
            queryParams.Add($"centerName={Uri.EscapeDataString(input.CenterName)}");

        if (!string.IsNullOrWhiteSpace(input.BAPSID))
            queryParams.Add($"BAPSID={Uri.EscapeDataString(input.BAPSID)}");

        // same pattern for other fields...

        var queryString = string.Join("&", queryParams);

        var url = string.IsNullOrEmpty(queryString)
            ? $"{baseUrl}{endpoint}"
            : $"{baseUrl}{endpoint}?{queryString}";

        Console.WriteLine(url); // debug

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("x-baps-auth-app-id", config["MisApi:AppId"]);
        request.Headers.Add("x-baps-auth-app-secret", config["MisApi:AppSecret"]);
        request.Headers.Add("User-Agent", "ALM/dev");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new GraphQLException(await response.Content.ReadAsStringAsync());

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<IEnumerable<PersonSearch>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? new List<PersonSearch>();
    }
}