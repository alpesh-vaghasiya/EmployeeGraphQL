using Xunit;
using Api.GraphQL;
using EmployeeGraphQL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Moq;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;
using HotChocolate;

public class TemplateMutationTests_Unit
{
  [Fact]
  public void MapInputToEntity_ValidInput_ShouldMapBasicFields()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test Template",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      LocationScopeIds = "[1,2]"
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity);
    Assert.Equal("Test Template", entity.Title);
    Assert.Equal("DRAFT", entity.Status);
    Assert.Equal(1, entity.ProjectTypeId);
    Assert.Equal(1, entity.SamparkTypeId);
    Assert.Equal("[1,2]", entity.LocationScopeIds);
  }

  [Fact]
  public void MapInputToEntity_ShouldConvertDateTimeToDateOnly()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var startDate = DateTime.UtcNow;
    var endDate = DateTime.UtcNow.AddDays(2);

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      StartDate = startDate,
      EndDate = endDate
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity.StartDate);
    Assert.NotNull(entity.EndDate);

    Assert.Equal(DateOnly.FromDateTime(startDate), entity.StartDate);
    Assert.Equal(DateOnly.FromDateTime(endDate), entity.EndDate);
  }

  //Null Date Handling (Negative Test)
  [Fact]
  public void MapInputToEntity_NullDates_ShouldRemainNull()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      StartDate = null,
      EndDate = null
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.Null(entity.StartDate);
    Assert.Null(entity.EndDate);
  }

  //JSON Serialization Test
  [Fact]
  public void MapInputToEntity_ShouldSerializeProjectFrequencyConfig()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      ProjectRepeateFrequencyConfig = new ProjectFrequencyInput
      {
        Type = "repeat",
        CreateProjectTimes = 3,
        RepeatEvery = 2,
        RepeatUnit = "days"
      }
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity.ProjectRepeateFrequencyConfig);
    Assert.Contains("repeat", entity.ProjectRepeateFrequencyConfig);
    Assert.Contains("CreateProjectTimes", entity.ProjectRepeateFrequencyConfig);
  }

  //JSON Null Handling (Negative Test)
  [Fact]
  public void MapInputToEntity_NullProjectFrequencyConfig_ShouldSetNull()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      ProjectRepeateFrequencyConfig = null
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.Null(entity.ProjectRepeateFrequencyConfig);
  }

  // TargetConfigs → Add New (Positive Case)

  [Fact]
  public void MapInputToEntity_ShouldAddTargetConfigs()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      TargetConfigs = new List<TemplateTargetConfigInput>
        {
            new TemplateTargetConfigInput
            {
                ConfigType = "TYPE1",
                WingMale = true,
                WingFemale = false
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity.TargetConfigs);
    Assert.Single(entity.TargetConfigs);

    var config = entity.TargetConfigs.First();

    Assert.Equal("TYPE1", config.ConfigType);
    Assert.True(config.WingMale);
    Assert.False(config.WingFemale);
    Assert.Equal("ACTIVE", config.Status);
  }

  //TargetConfigs → Update Existing
  [Fact]
  public void MapInputToEntity_ShouldUpdateExistingTargetConfig()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      TargetConfigs = new List<TemplateTargetConfig>
        {
            new TemplateTargetConfig
            {
                ConfigType = "TYPE1",
                WingMale = false,
                WingFemale = false,
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      TargetConfigs = new List<TemplateTargetConfigInput>
        {
            new TemplateTargetConfigInput
            {
                ConfigType = "TYPE1",
                WingMale = true,
                WingFemale = true
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Single(entity.TargetConfigs);

    var config = entity.TargetConfigs.First();

    Assert.True(config.WingMale);
    Assert.True(config.WingFemale);
    Assert.Equal("ACTIVE", config.Status);
  }

  // TargetConfigs → Remove Existing (Negative Case)
  [Fact]
  public void MapInputToEntity_ShouldMarkRemovedTargetConfigsAsInactive()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      TargetConfigs = new List<TemplateTargetConfig>
        {
            new TemplateTargetConfig
            {
                ConfigType = "TYPE1",
                Status = "ACTIVE"
            },
            new TemplateTargetConfig
            {
                ConfigType = "TYPE2",
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      TargetConfigs = new List<TemplateTargetConfigInput>
        {
            new TemplateTargetConfigInput
            {
                ConfigType = "TYPE1" // only TYPE1 present
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Equal(2, entity.TargetConfigs.Count);

    var activeConfig = entity.TargetConfigs.First(x => x.ConfigType == "TYPE1");
    var inactiveConfig = entity.TargetConfigs.First(x => x.ConfigType == "TYPE2");

    Assert.Equal("ACTIVE", activeConfig.Status);
    Assert.Equal("INACTIVE", inactiveConfig.Status);
  }

  //DepartmentConfigs → Add New (Positive)
  [Fact]
  public void MapInputToEntity_ShouldAddDepartmentConfigs()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      DepartmentConfigs = new List<TemplateDepartmentConfigInput>
        {
            new TemplateDepartmentConfigInput
            {
                DepartmentId = 10,
                OwnerRoleId = 5,
                IsPrimary = true
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity.DepartmentConfigs);
    Assert.Single(entity.DepartmentConfigs);

    var config = entity.DepartmentConfigs.First();

    Assert.Equal(10, config.DepartmentId);
    Assert.Equal(5, config.OwnerRoleId);
    Assert.True(config.IsPrimary);
    Assert.Equal("ACTIVE", config.Status);
    Assert.NotEqual(Guid.Empty, config.DepartmentConfigUucode);
  }

  //DepartmentConfigs → Update Existing
  [Fact]
  public void MapInputToEntity_ShouldUpdateExistingDepartmentConfig()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      DepartmentConfigs = new List<TemplateDepartmentConfig>
        {
            new TemplateDepartmentConfig
            {
                DepartmentId = 10,
                OwnerRoleId = 1,
                IsPrimary = false,
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      DepartmentConfigs = new List<TemplateDepartmentConfigInput>
        {
            new TemplateDepartmentConfigInput
            {
                DepartmentId = 10,
                OwnerRoleId = 99,
                IsPrimary = true
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Single(entity.DepartmentConfigs);

    var config = entity.DepartmentConfigs.First();

    Assert.Equal(10, config.DepartmentId);
    Assert.Equal(99, config.OwnerRoleId);
    Assert.True(config.IsPrimary);
    Assert.Equal("ACTIVE", config.Status);
  }

  // DepartmentConfigs → Mark Removed as INACTIVE
  [Fact]
  public void MapInputToEntity_ShouldMarkRemovedDepartmentConfigsAsInactive()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      DepartmentConfigs = new List<TemplateDepartmentConfig>
        {
            new TemplateDepartmentConfig
            {
                DepartmentId = 10,
                Status = "ACTIVE"
            },
            new TemplateDepartmentConfig
            {
                DepartmentId = 20,
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      DepartmentConfigs = new List<TemplateDepartmentConfigInput>
        {
            new TemplateDepartmentConfigInput
            {
                DepartmentId = 10 // only this remains
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Equal(2, entity.DepartmentConfigs.Count);

    var active = entity.DepartmentConfigs.First(x => x.DepartmentId == 10);
    var inactive = entity.DepartmentConfigs.First(x => x.DepartmentId == 20);

    Assert.Equal("ACTIVE", active.Status);
    Assert.Equal("INACTIVE", inactive.Status);
  }

  //TargetSurveys → Add New (Positive)
  [Fact]
  public void MapInputToEntity_ShouldAddTargetSurveys()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      TargetSurveys = new List<TemplateTargetSurveyInput>
        {
            new TemplateTargetSurveyInput
            {
                GssFormId = "100", // ✅ FIX
                ConfigType = "TYPE1",
                IsRequired = true
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity.TargetSurveys);
    Assert.Single(entity.TargetSurveys);

    var survey = entity.TargetSurveys.First();

    Assert.Equal("100", survey.GssFormId);
    Assert.Equal("TYPE1", survey.ConfigType);
    Assert.True(survey.IsRequired);
    Assert.Equal("ACTIVE", survey.Status);
  }

  [Fact]
  public void MapInputToEntity_ShouldUpdateExistingTargetSurvey_ByCompositeKey()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      TargetSurveys = new List<TemplateTargetSurvey>
        {
            new TemplateTargetSurvey
            {
                GssFormId = "100",
                ConfigType = "TYPE1",
                IsRequired = false,
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      TargetSurveys = new List<TemplateTargetSurveyInput>
        {
            new TemplateTargetSurveyInput
            {
                GssFormId = "100",
                ConfigType = "TYPE1",
                IsRequired = true // update value
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Single(entity.TargetSurveys);

    var survey = entity.TargetSurveys.First();

    Assert.Equal("100", survey.GssFormId);
    Assert.Equal("TYPE1", survey.ConfigType);
    Assert.True(survey.IsRequired); // ✅ updated
    Assert.Equal("ACTIVE", survey.Status);
  }

  // TargetSurveys → Mark Removed as INACTIVE (CRITICAL)
  //   if (!input.TargetSurveys.Any(x =>
  //     x.GssFormId == db.GssFormId &&
  //     x.ConfigType == db.ConfigType))
  // {
  //     db.Status = "INACTIVE";
  // }
  [Fact]
  public void MapInputToEntity_ShouldMarkRemovedTargetSurveysAsInactive()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      TargetSurveys = new List<TemplateTargetSurvey>
        {
            new TemplateTargetSurvey
            {
                GssFormId = "100",
                ConfigType = "TYPE1",
                Status = "ACTIVE"
            },
            new TemplateTargetSurvey
            {
                GssFormId = "200",
                ConfigType = "TYPE2",
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      TargetSurveys = new List<TemplateTargetSurveyInput>
        {
            new TemplateTargetSurveyInput
            {
                GssFormId = "100",
                ConfigType = "TYPE1"
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Equal(2, entity.TargetSurveys.Count);

    var active = entity.TargetSurveys.First(x => x.GssFormId == "100");
    var inactive = entity.TargetSurveys.First(x => x.GssFormId == "200");

    Assert.Equal("ACTIVE", active.Status);
    Assert.Equal("INACTIVE", inactive.Status);
  }

  // Documents → Add New
  [Fact]
  public void MapInputToEntity_ShouldAddDocuments()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      Documents = new List<TemplateDocumentInput>
        {
            new TemplateDocumentInput
            {
                DocumentName = "Doc1",
                DocumentUrl = "url1",
                DocumentType = "PDF",
                FileSize = 100
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input);

    // Assert
    Assert.NotNull(entity.Documents);
    Assert.Single(entity.Documents);

    var doc = entity.Documents.First();

    Assert.Equal("Doc1", doc.DocumentName);
    Assert.Equal("url1", doc.DocumentUrl);
    Assert.Equal("PDF", doc.DocumentType);
    Assert.Equal(100, doc.FileSize);
    Assert.Equal("ACTIVE", doc.Status);
    Assert.NotEqual(Guid.Empty, doc.DocumentUucode);
  }
  //Documents → Update Existing
  [Fact]
  public void MapInputToEntity_ShouldUpdateExistingDocument()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      Documents = new List<TemplateDocument>
        {
            new TemplateDocument
            {
                DocumentName = "Doc1",
                DocumentUrl = "old-url",
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      Documents = new List<TemplateDocumentInput>
        {
            new TemplateDocumentInput
            {
                DocumentName = "Doc1",
                DocumentUrl = "new-url"
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Single(entity.Documents);

    var doc = entity.Documents.First();

    Assert.Equal("Doc1", doc.DocumentName);
    Assert.Equal("new-url", doc.DocumentUrl); // ✅ updated
    Assert.Equal("ACTIVE", doc.Status);
  }
  //Documents → Mark Removed as INACTIVE
  [Fact]
  public void MapInputToEntity_ShouldMarkRemovedDocumentsAsInactive()
  {
    // Arrange
    var mutation = new TemplateMutation();

    var existing = new Template
    {
      Documents = new List<TemplateDocument>
        {
            new TemplateDocument
            {
                DocumentName = "Doc1",
                Status = "ACTIVE"
            },
            new TemplateDocument
            {
                DocumentName = "Doc2",
                Status = "ACTIVE"
            }
        }
    };

    var input = new TemplateInput
    {
      Title = "Test",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      Documents = new List<TemplateDocumentInput>
        {
            new TemplateDocumentInput
            {
                DocumentName = "Doc1"
            }
        }
    };

    // Act
    var entity = InvokeMap(mutation, input, existing);

    // Assert
    Assert.Equal(2, entity.Documents.Count);

    var active = entity.Documents.First(x => x.DocumentName == "Doc1");
    var inactive = entity.Documents.First(x => x.DocumentName == "Doc2");

    Assert.Equal("ACTIVE", active.Status);
    Assert.Equal("INACTIVE", inactive.Status);
  }

  [Fact]
  public async Task CreateTemplate_ValidInput_ShouldSaveAndReturnEntity()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    var db = new AppDbContext(options);

    var validator = new Mock<IValidator<TemplateInput>>();
    validator.Setup(v => v.ValidateAsync(It.IsAny<TemplateInput>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new FluentValidation.Results.ValidationResult(
        new List<FluentValidation.Results.ValidationFailure>()
    ));

    var mutation = new TemplateMutation();

    var input = new TemplateInput
    {
      Title = "Test Template",
      ProjectTypeId = 1,
      SamparkTypeId = 1,
      AllowedDraftProject = "YES"
    };

    // Act
    var result = await mutation.CreateTemplate(input, db, validator.Object, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test Template", result.Title);

    // DB check 🔥
    var saved = db.Set<Template>().FirstOrDefault();
    Assert.NotNull(saved);
    Assert.Equal("Test Template", saved.Title);
  }

  [Fact]
  public async Task CreateTemplate_InvalidInput_ShouldThrowException()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    var db = new AppDbContext(options);

    var validator = new Mock<IValidator<TemplateInput>>();
    validator.Setup(v => v.ValidateAsync(It.IsAny<TemplateInput>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new FluentValidation.Results.ValidationResult(
            new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Title", "Title required")
            }
        ));

    var mutation = new TemplateMutation();

    var input = new TemplateInput(); // invalid

    // Act & Assert
    await Assert.ThrowsAsync<GraphQLException>(() =>
        mutation.CreateTemplate(input, db, validator.Object, CancellationToken.None));
  }

  private Template InvokeMap(TemplateMutation mutation, TemplateInput input, Template? existing = null)
  {
    return mutation
        .GetType()
        .GetMethod("MapInputToEntity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
        .Invoke(mutation, new object[] { input, existing }) as Template;
  }

  //   Fully Covered
  //  Basic mapping
  // Date conversion
  //  JSON serialization
  //  TargetConfigs (Add/Update/Inactive)
  //  DepartmentConfigs (Add/Update/Inactive)
  //  TargetSurveys (Add/Update/Inactive)
  //  Documents (Add/Update/Inactive)
}