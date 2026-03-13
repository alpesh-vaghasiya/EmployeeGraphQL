using Api.GraphQL.Inputs;
using Dapper;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly IValidator<ProjectInput> _validator;
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public ProjectService(AppDbContext db, IValidator<ProjectInput> validator, IConfiguration config)
    {
        _db = db;
        _validator = validator;
        _config = config;
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<PagedResult<ProjectResponse>> Projects(long departmentId, QueryOptions options)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var offset = (options.Page - 1) * options.PageSize;

        var sortColumn = options.SortBy?.ToLower() switch
        {
            "name" => "p.title",
            "startdate" => "p.project_start_date",
            "enddate" => "p.project_end_date",
            "status" => "p.status",
            _ => "p.created_at"
        };

        var sortOrder = options.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC";

        var sql = string.Format(ProjectQueries.GetProjects, sortColumn, sortOrder);

        var parameters = new
        {
            DepartmentId = departmentId,
            Search = options.Search,
            Status = options.Status,
            PageSize = options.PageSize,
            Offset = offset
        };

        return await connection.QueryPagedAsync<ProjectResponse>(
            sql,
            parameters,
            options.Page,
            options.PageSize
        );
    }

    // CREATE
    public async Task<Project> CreateProject(ProjectInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Validation failed").SetCode("VALIDATION_ERROR").SetExtension("errors", errors).Build());
        }

        var name = input.Name.Trim();

        if (await _db.Projects.AnyAsync(x => EF.Functions.ILike(x.Title, name), cancellationToken))
        {
            throw new GraphQLException("Project title already exists.");
        }

        var template = await _db.Templates
            .FirstOrDefaultAsync(x => x.TemplateId == input.TemplateId, cancellationToken);

        if (template == null)
            throw new GraphQLException("Template not found");

        var startDate = input.ProjectStartDate ?? template.StartDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow;
        var endDate = input.ProjectEndDate ?? template.EndDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow;

        var project = new Project
        {
            ProjectUucode = Guid.NewGuid(),
            Status = "DRAFT",
            Title = name,
            TemplateId = input.TemplateId,
            ProjectStartDate = startDate,
            ProjectEndDate = endDate,
            Description = input.Description,
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        return project;
    }

    // UPDATE
    public async Task<Project> UpdateProject(int id, ProjectInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Validation failed").SetCode("VALIDATION_ERROR").SetExtension("errors", errors).Build());
        }

        var project = await _db.Projects.FirstOrDefaultAsync(x => x.ProjectId == id, cancellationToken);

        if (project == null)
            throw new GraphQLException("Project not found");

        var name = input.Name.Trim();

        if (await _db.Projects.AnyAsync(x => EF.Functions.ILike(x.Title, name) && x.ProjectId != id, cancellationToken))
        {
            throw new GraphQLException("Project title already exists.");
        }

        project.Title = name;
        project.Description = input.Description;
        project.ProjectStartDate = input.ProjectStartDate ?? project.ProjectStartDate;
        project.ProjectEndDate = input.ProjectEndDate ?? project.ProjectEndDate;
        project.ReminderFrequency = input.ReminderFrequency;
        project.ReminderFrequencyConfig = input.ReminderFrequencyConfig;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return project;
    }

    // DELETE
    public async Task<bool> DeleteProject(int id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.ProjectId == id, cancellationToken);

        if (project == null)
            throw new GraphQLException("Project not found");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}