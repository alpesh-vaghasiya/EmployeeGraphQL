using System.Text.Json;
using Api.GraphQL.Inputs;
using Dapper;
using Domain.Entities;
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
    private readonly ReminderService _reminderService;

    public ProjectService(AppDbContext db, IValidator<ProjectInput> validator, IConfiguration config, ReminderService reminderService)
    {
        _db = db;
        _validator = validator;
        _config = config;
        _connectionString = config.GetConnectionString("DefaultConnection");
        _reminderService = reminderService;
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
        var normalizedName = name.ToLowerInvariant();

        if (await _db.Projects.AnyAsync(x => (x.Title.ToLower() == normalizedName && x.LocationId == input.LocationId.ToString()), cancellationToken))
        {
            throw new GraphQLException("Project title already exists.");
        }

        var template = await _db.Templates
            .FirstOrDefaultAsync(x => x.TemplateId == input.TemplateId, cancellationToken);

        if (template == null)
            throw new GraphQLException("Template not found");

        // Convert template dates to DateTime
        var templateStart = template.StartDate?.ToDateTime(TimeOnly.MinValue);
        var templateEnd = template.EndDate?.ToDateTime(TimeOnly.MinValue);

        // Validate only if input dates are provided
        if (input.ProjectStartDate.HasValue && templateStart.HasValue)
        {
            if (input.ProjectStartDate < templateStart)
            {
                throw new GraphQLException("Project start date cannot be before template start date.");
            }
        }

        if (input.ProjectEndDate.HasValue && templateEnd.HasValue)
        {
            if (input.ProjectEndDate > templateEnd)
            {
                throw new GraphQLException("Project end date cannot be after template end date.");
            }
        }

        // Optional: start < end validation
        if (input.ProjectStartDate.HasValue && input.ProjectEndDate.HasValue)
        {
            if (input.ProjectStartDate > input.ProjectEndDate)
            {
                throw new GraphQLException("Project start date cannot be greater than end date.");
            }
        }

        var startDate = input.ProjectStartDate
     ?? (template.StartDate.HasValue
         ? DateTime.SpecifyKind(
             template.StartDate.Value.ToDateTime(TimeOnly.MinValue),
             DateTimeKind.Utc)
         : DateTime.UtcNow);

        var endDate = input.ProjectEndDate
            ?? (template.EndDate.HasValue
                ? DateTime.SpecifyKind(
                    template.EndDate.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc)
                : DateTime.UtcNow);

        var reminderFrequencyConfig = template.ReminderFrequencyConfig;

        // if (template.CustomReminder == true)
        // {
        //     reminderFrequencyConfig = input.ReminderFrequencyConfig != null ? JsonSerializer.Serialize(input.ReminderFrequencyConfig) : null;
        // }
        var project = new Project
        {
            ProjectUucode = Guid.NewGuid(),
            Status = "DRAFT",
            Title = name,
            TemplateId = input.TemplateId,
            ProjectStartDate = startDate,
            ProjectEndDate = endDate,
            Description = input.Description,
            ReminderFrequency = input.ReminderFrequency,
            ReminderFrequencyConfig = reminderFrequencyConfig,
            LocationId = input.LocationId?.ToString(),
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        return project;
    }

    // UPDATE
    public async Task<Project> UpdateProject(long id, ProjectInput input, CancellationToken cancellationToken)
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
        var normalizedName = name.ToLowerInvariant();

        if (await _db.Projects.AnyAsync(x => x.Title.ToLower() == normalizedName && x.ProjectId != id, cancellationToken))
        {
            throw new GraphQLException("Project title already exists.");
        }

        project.Title = name;
        project.Description = input.Description;
        project.ProjectStartDate = input.ProjectStartDate ?? project.ProjectStartDate;
        project.ProjectEndDate = input.ProjectEndDate ?? project.ProjectEndDate;
        project.ReminderFrequency = input.ReminderFrequency;
        project.ReminderFrequencyConfig = input.ReminderFrequencyConfig != null ? JsonSerializer.Serialize(input.ReminderFrequencyConfig) : project.ReminderFrequencyConfig;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return project;
    }

    // DELETE
    public async Task<bool> DeleteProject(long id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.ProjectId == id, cancellationToken);

        if (project == null)
            throw new GraphQLException("Project not found");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
    public async Task<Project> PublishProject(long id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(x => x.ProjectId == id, cancellationToken);

        if (project == null)
            throw new GraphQLException("Project not found");

        if (project.Status == "PUBLISH")
            throw new GraphQLException("Project already published");

        project.Status = "PUBLISH";

        // generate reminder schedules
        var reminderDates = _reminderService.GenerateDates(project);

        var config = string.IsNullOrEmpty(project.ReminderFrequencyConfig) ? null : JsonSerializer.Deserialize<ReminderConfigInput>(project.ReminderFrequencyConfig!);

        TimeSpan? time = null;

        if (!string.IsNullOrEmpty(project.ReminderFrequencyConfig))
        {
            if (!string.IsNullOrEmpty(config?.Time))
            {
                time = TimeSpan.Parse(config.Time);
            }
        }

        foreach (var date in reminderDates)
        {
            _db.ProjectSchedules.Add(new ProjectSchedule
            {
                TemplateId = (int)project.TemplateId,
                ProjectId = (int)project.ProjectId,
                ScheduleType = "REMINDER",
                ScheduledDate = DateOnly.FromDateTime(date),
                Status = "PENDING",
                ScheduledTime = time,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return project;
    }
}