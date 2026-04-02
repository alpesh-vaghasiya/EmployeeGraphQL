using Api.GraphQL.Inputs;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Test-only IProjectService that replaces Dapper-based Projects() with an
/// EF Core query so integration tests can run entirely on InMemory database.
/// All mutating operations delegate to the real ProjectService.
/// </summary>
public class TestProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly ProjectService _inner;

    public TestProjectService(
        AppDbContext db,
        IValidator<ProjectInput> validator,
        IConfiguration config,
        ReminderService reminderService)
    {
        _db = db;
        _inner = new ProjectService(db, validator, config, reminderService);
    }

    /// <summary>
    /// EF Core replacement for the Dapper-based SQL query in ProjectService.
    /// Supports search, status filter, and pagination matching the real query contract.
    /// </summary>
    public async Task<PagedResult<ProjectResponse>> Projects(long departmentId, QueryOptions options)
    {
        var query = _db.Projects
            .Include(p => p.Template)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(options.Search))
            query = query.Where(p => p.Title.Contains(options.Search));

        if (!string.IsNullOrWhiteSpace(options.Status))
            query = query.Where(p => p.Status == options.Status);

        var total = await query.CountAsync();

        var page     = options.Page < 1 ? 1 : options.Page;
        var pageSize = options.PageSize < 1 ? 10 : options.PageSize;
        var offset   = (page - 1) * pageSize;

        var sortedQuery = options.SortBy?.ToLower() switch
        {
            "name"      => options.SortOrder?.ToLower() == "asc"
                               ? query.OrderBy(p => p.Title)
                               : query.OrderByDescending(p => p.Title),
            "startdate" => options.SortOrder?.ToLower() == "asc"
                               ? query.OrderBy(p => p.ProjectStartDate)
                               : query.OrderByDescending(p => p.ProjectStartDate),
            "enddate"   => options.SortOrder?.ToLower() == "asc"
                               ? query.OrderBy(p => p.ProjectEndDate)
                               : query.OrderByDescending(p => p.ProjectEndDate),
            "status"    => options.SortOrder?.ToLower() == "asc"
                               ? query.OrderBy(p => p.Status)
                               : query.OrderByDescending(p => p.Status),
            _           => query.OrderByDescending(p => p.ProjectId),
        };

        var items = await sortedQuery
            .Skip(offset)
            .Take(pageSize)
            .Select(p => new ProjectResponse
            {
                ProjectId     = p.ProjectId,
                ProjectName   = p.Title,
                SamparkType   = p.Template != null ? p.Template.SamparkTypeId.ToString() : null,
                KaryakarCount = 0,
                FamilyCount   = 0,
                StartDate     = p.ProjectStartDate,
                EndDate       = p.ProjectEndDate,
                CreatedBy     = p.CreatedBy,
                Status        = p.Status,
            })
            .ToListAsync();

        return new PagedResult<ProjectResponse>
        {
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            Items      = items,
        };
    }

    public Task<Project> CreateProject(ProjectInput input, CancellationToken cancellationToken)
        => _inner.CreateProject(input, cancellationToken);

    public Task<Project> UpdateProject(long id, ProjectInput input, CancellationToken cancellationToken)
        => _inner.UpdateProject(id, input, cancellationToken);

    public Task<bool> DeleteProject(long id, CancellationToken cancellationToken)
        => _inner.DeleteProject(id, cancellationToken);

    public Task<Project> PublishProject(long id, CancellationToken cancellationToken)
        => _inner.PublishProject(id, cancellationToken);
}
