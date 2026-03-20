using System.Text.Json;
using Api.GraphQL.Inputs;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

public class ProjectServiceTests
{
    private static AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);      
    }

    private static ProjectService CreateService(AppDbContext dbContext, Mock<IValidator<ProjectInput>> validatorMock = null)
    {
        var validator = validatorMock ?? new Mock<IValidator<ProjectInput>>();

        if (validatorMock == null)
        {
            validator.Setup(v => v.ValidateAsync(It.IsAny<ProjectInput>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        var inMemConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;"
            })
            .Build();

        var reminderService = new Mock<ReminderService>();

        return new ProjectService(dbContext, validator.Object, inMemConfig, reminderService.Object);
    }

    [Fact]
    public async Task CreateProject_ValidInput_ReturnsProjectWithTemplateDates()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var template = new Template
        {
            TemplateId = 1,
            TemplateUucode = Guid.NewGuid(),
            Title = "T1",
            AllowedDraftProject = "[]",
            ProjectTypeId = 1,
            SamparkTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10)),
            ReminderFrequencyConfig = "{\"frequency\":\"ONCE\",\"time\":\"10:00:00\"}" // optional
        };

        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var input = new ProjectInput
        {
            Name = "  My New Project  ",
            TemplateId = template.TemplateId,
            Description = "Desc",
            ReminderFrequency = "ONCE"
        };

        var createdProject = await service.CreateProject(input, CancellationToken.None);

        Assert.NotNull(createdProject);
        Assert.Equal("DRAFT", createdProject.Status);
        Assert.Equal("My New Project", createdProject.Title);
        Assert.Equal(template.TemplateId, createdProject.TemplateId);
        Assert.Equal("Desc", createdProject.Description);
        Assert.Equal("ONCE", createdProject.ReminderFrequency);
        Assert.Equal(template.ReminderFrequencyConfig, createdProject.ReminderFrequencyConfig);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)).ToDateTime(TimeOnly.MinValue), createdProject.ProjectStartDate);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10)).ToDateTime(TimeOnly.MinValue), createdProject.ProjectEndDate);

        var found = await dbContext.Projects.FindAsync(createdProject.ProjectId);
        Assert.NotNull(found);
    }

    [Fact]
    public async Task CreateProject_WithExplicitDates_UsesInputDates()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var template = new Template
        {
            TemplateId = 1,
            TemplateUucode = Guid.NewGuid(),
            Title = "T1",
            AllowedDraftProject = "[]",
            ProjectTypeId = 1,
            SamparkTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)),
            ReminderFrequencyConfig = "{\"frequency\":\"REPEAT\",\"time\":\"09:00:00\"}"
        };

        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var explicitStart = DateTime.UtcNow.Date.AddDays(3).AddHours(1);
        var explicitEnd = DateTime.UtcNow.Date.AddDays(4).AddHours(1);

        var input = new ProjectInput
        {
            Name = "Explicit",
            TemplateId = template.TemplateId,
            ProjectStartDate = explicitStart,
            ProjectEndDate = explicitEnd,
            Description = "X"
        };

        var createdProject = await service.CreateProject(input, CancellationToken.None);

        Assert.Equal(explicitStart, createdProject.ProjectStartDate);
        Assert.Equal(explicitEnd, createdProject.ProjectEndDate);
    }

    // TODO: Add tests for explicit dates negative
    [Fact]
    public async Task CreateProject_WithExplicitDatesOutsideTemplateRange_ShouldThrowException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var template = new Template
        {
            TemplateId = 1,
            TemplateUucode = Guid.NewGuid(),
            Title = "T1",
            AllowedDraftProject = "[]",
            ProjectTypeId = 1,
            SamparkTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5))
        };

        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var explicitStart = DateTime.UtcNow.Date.AddDays(3);
        var explicitEnd = DateTime.UtcNow.Date.AddDays(8); // ❌ outside template

        var input = new ProjectInput
        {
            Name = "Invalid Explicit",
            TemplateId = template.TemplateId,
            ProjectStartDate = explicitStart,
            ProjectEndDate = explicitEnd
        };

        await Assert.ThrowsAsync<GraphQLException>(() =>
            service.CreateProject(input, CancellationToken.None));
    }

    // TODO: another negative - start before template
    [Fact]
    public async Task CreateProject_WithStartDateBeforeTemplate_ShouldThrowException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var template = new Template
        {
            TemplateId = 1,
            TemplateUucode = Guid.NewGuid(),
            Title = "T1",
            AllowedDraftProject = "[]",
            ProjectTypeId = 1,
            SamparkTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5))
        };

        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var input = new ProjectInput
        {
            Name = "Invalid Start",
            TemplateId = template.TemplateId,
            ProjectStartDate = DateTime.UtcNow.Date.AddDays(1), // ❌ before template
            ProjectEndDate = DateTime.UtcNow.Date.AddDays(3)
        };

        await Assert.ThrowsAsync<GraphQLException>(() =>
            service.CreateProject(input, CancellationToken.None));
    }
    [Fact]
    public async Task CreateProject_TemplateNotFound_ThrowsGraphQLException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var service = CreateService(dbContext);

        var input = new ProjectInput
        {
            Name = "Bad",
            TemplateId = 99
        };

        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.CreateProject(input, CancellationToken.None));
        Assert.Equal("Template not found", ex.Message);
    }

    [Fact]
    public async Task CreateProject_DuplicateTitle_ThrowsGraphQLException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var template = new Template
        {
            TemplateId = 1,
            TemplateUucode = Guid.NewGuid(),
            Title = "T1",
            AllowedDraftProject = "[]",
            ProjectTypeId = 1,
            SamparkTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5))
        };

        dbContext.Templates.Add(template);

        dbContext.Projects.Add(new Project
        {
            ProjectUucode = Guid.NewGuid(),
            TemplateId = template.TemplateId,
            Title = "Existing Project",
            Status = "DRAFT",
            ProjectStartDate = DateTime.UtcNow,
            ProjectEndDate = DateTime.UtcNow.AddDays(1),
            Description = "Old"
        });

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var input = new ProjectInput
        {
            Name = "existing project", // case difference
            TemplateId = template.TemplateId,
        };

        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.CreateProject(input, CancellationToken.None));
        Assert.Equal("Project title already exists.", ex.Message);
    }

    [Fact]
    public async Task CreateProject_InvalidInput_ThrowsGraphQLExceptionWithValidationErrors()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var validatorMock = new Mock<IValidator<ProjectInput>>();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("TemplateId", "Template is required")
        };

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ProjectInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var service = CreateService(dbContext, validatorMock);

        var input = new ProjectInput
        {
            Name = "",
            TemplateId = 0
        };

        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.CreateProject(input, CancellationToken.None));

        var graphQLError = ex.Errors.First();
        Assert.Equal("Validation failed", graphQLError.Message);

        var errors = graphQLError.Extensions["errors"] as List<string>;
        Assert.NotNull(errors);
        Assert.Contains("Name is required", errors);
        Assert.Contains("Template is required", errors);
    }

    [Fact]
    public async Task CreateProject_TemplateNoDates_DefaultsToUtcNowRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var template = new Template
        {
            TemplateId = 1,
            TemplateUucode = Guid.NewGuid(),
            Title = "T1",
            AllowedDraftProject = "[]",
            ProjectTypeId = 1,
            SamparkTypeId = 1,
            StartDate = null,
            EndDate = null,
            ReminderFrequencyConfig = null
        };

        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var before = DateTime.UtcNow;
        var createdProject = await service.CreateProject(new ProjectInput { Name = "NoDates", TemplateId = template.TemplateId }, CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.True(createdProject.ProjectStartDate >= before);
        Assert.True(createdProject.ProjectStartDate <= after);
        Assert.True(createdProject.ProjectEndDate >= before);
        Assert.True(createdProject.ProjectEndDate <= after);
    }

    [Fact]
    public async Task UpdateProject_ValidInput_UpdatesProjectFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var project = new Project
        {
            ProjectUucode = Guid.NewGuid(),
            TemplateId = 100,
            Title = "Original Title",
            Description = "Original",
            Status = "DRAFT",
            ProjectStartDate = DateTime.UtcNow.Date.AddDays(1),
            ProjectEndDate = DateTime.UtcNow.Date.AddDays(5),
            ReminderFrequency = "DAILY",
            ReminderFrequencyConfig = "{\"frequency\":\"DAILY\",\"time\":\"08:00:00\"}"
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var updateInput = new ProjectInput
        {
            Name = "  Updated Title  ",
            TemplateId = project.TemplateId,
            Description = "Updated desc",
            ProjectStartDate = project.ProjectStartDate.AddDays(1),
            ProjectEndDate = project.ProjectEndDate.AddDays(1),
            ReminderFrequency = "WEEKLY",
            ReminderFrequencyConfig = new ReminderConfigInput { Frequency = "WEEKLY", Time = "09:00:00" }
        };

        var updatedProject = await service.UpdateProject(project.ProjectId, updateInput, CancellationToken.None);

        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal("Updated desc", updatedProject.Description);
        Assert.Equal(updateInput.ProjectStartDate, updatedProject.ProjectStartDate);
        Assert.Equal(updateInput.ProjectEndDate, updatedProject.ProjectEndDate);
        Assert.Equal("WEEKLY", updatedProject.ReminderFrequency);
        Assert.True(!string.IsNullOrEmpty(updatedProject.ReminderFrequencyConfig));

        var parsedConfig = JsonSerializer.Deserialize<ReminderConfigInput>(updatedProject.ReminderFrequencyConfig!);
        Assert.NotNull(parsedConfig);
        Assert.Equal("WEEKLY", parsedConfig!.Frequency);
        Assert.Equal("09:00:00", parsedConfig.Time);

        Assert.True(updatedProject.UpdatedAt.HasValue);

        var found = await dbContext.Projects.FindAsync(project.ProjectId);
        Assert.NotNull(found);
        Assert.Equal("Updated Title", found.Title);
        Assert.Equal("Updated desc", found.Description);
    }

    [Fact]
    public async Task UpdateProject_IdNotFound_ThrowsGraphQLException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var service = CreateService(dbContext);

        var input = new ProjectInput
        {
            Name = "Doesnt Matter",
            TemplateId = 10
        };

        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.UpdateProject(999, input, CancellationToken.None));
        Assert.Equal("Project not found", ex.Message);
    }

    [Fact]
    public async Task UpdateProject_DuplicateTitle_ThrowsGraphQLException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        dbContext.Projects.Add(new Project
        {
            ProjectUucode = Guid.NewGuid(),
            TemplateId = 1,
            Title = "Existing Project",
            Status = "DRAFT",
            ProjectStartDate = DateTime.UtcNow,
            ProjectEndDate = DateTime.UtcNow.AddDays(1),
        });

        dbContext.Projects.Add(new Project
        {
            ProjectUucode = Guid.NewGuid(),
            TemplateId = 1,
            Title = "ToUpdate",
            Status = "DRAFT",
            ProjectStartDate = DateTime.UtcNow,
            ProjectEndDate = DateTime.UtcNow.AddDays(1),
        });

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var input = new ProjectInput
        {
            Name = "existing project",
            TemplateId = 1
        };

        var secondProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "ToUpdate");
        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.UpdateProject(secondProject.ProjectId, input, CancellationToken.None));

        Assert.Equal("Project title already exists.", ex.Message);
    }

    [Fact]
    public async Task UpdateProject_InvalidInput_ThrowsGraphQLExceptionWithValidationErrors()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var project = new Project
        {
            ProjectUucode = Guid.NewGuid(),
            TemplateId = 1,
            Title = "ToUpdate",
            Status = "DRAFT",
            ProjectStartDate = DateTime.UtcNow,
            ProjectEndDate = DateTime.UtcNow.AddDays(1)
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var validatorMock = new Mock<IValidator<ProjectInput>>();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ProjectInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var service = CreateService(dbContext, validatorMock);

        var updateInput = new ProjectInput
        {
            Name = "",
            TemplateId = 1
        };

        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.UpdateProject(project.ProjectId, updateInput, CancellationToken.None));

        var graphQLError = ex.Errors.First();
        Assert.Equal("Validation failed", graphQLError.Message);

        var errors = graphQLError.Extensions["errors"] as List<string>;
        Assert.NotNull(errors);
        Assert.Contains("Name is required", errors);
    }

    [Fact]
    public async Task DeleteProject_ExistingProject_ReturnsTrueAndDeletesProject()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var project = new Project
        {
            ProjectUucode = Guid.NewGuid(),
            TemplateId = 1,
            Title = "DeleteMe",
            Status = "DRAFT",
            ProjectStartDate = DateTime.UtcNow,
            ProjectEndDate = DateTime.UtcNow.AddDays(1),
            Description = "To be deleted"
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.DeleteProject(project.ProjectId, CancellationToken.None);

        Assert.True(result);

        var found = await dbContext.Projects.FindAsync(project.ProjectId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteProject_NonExistingProject_ThrowsGraphQLException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateInMemoryContext(dbName);

        var service = CreateService(dbContext);

        var ex = await Assert.ThrowsAsync<GraphQLException>(() => service.DeleteProject(999, CancellationToken.None));
        Assert.Equal("Project not found", ex.Message);
    }
}
