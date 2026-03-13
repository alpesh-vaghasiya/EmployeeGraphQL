using System.Text.Json;
using Domain.Entities;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Api.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class TemplateMutation : GenericMutation<Template, TemplateInput>
    {
        protected override IQueryable<Template> IncludeNavigation(IQueryable<Template> query)
        {
            return query
                .Include(e => e.TargetConfigs)
                .Include(e => e.DepartmentConfigs)
                .Include(e => e.TargetSurveys)
                .Include(e => e.Documents);
        }

        protected override Template MapInputToEntity(TemplateInput input, Template? existingEntity = null)
        {
            var entity = existingEntity ?? new Template
            {
                TemplateUucode = Guid.NewGuid()
            };

            entity.Title = input.Title;
            entity.Description = input.Description;
            entity.Status = "DRAFT";
            entity.ProjectTypeId = input.ProjectTypeId;
            entity.SamparkTypeId = input.SamparkTypeId;
            entity.LocationScopeIds = input.LocationScopeIds;
            entity.LocationLevelId = input.LocationLevelId;
            entity.AllowedDraftProject = input.AllowedDraftProject;
            entity.DefaultProjectCreation = input.DefaultProjectCreation;

            entity.StartDate = input.StartDate.HasValue
                ? DateOnly.FromDateTime(input.StartDate.Value)
                : null;

            entity.EndDate = input.EndDate.HasValue
                ? DateOnly.FromDateTime(input.EndDate.Value)
                : null;

            entity.ProjectRepeateFrequencyConfig = input.ProjectRepeateFrequencyConfig != null
        ? JsonSerializer.Serialize(input.ProjectRepeateFrequencyConfig)
        : null;
            entity.ReminderValue = input.ReminderValue;
            entity.ReminderFrequencyConfig = input.ReminderFrequencyConfig;
            entity.CustomReminder = input.CustomReminder;
            entity.CustomDocument = input.CustomDocument;

            // =====================================================
            // TARGET CONFIG
            // UNIQUE (template_id, config_type)
            // =====================================================

            if (input.TargetConfigs != null)
            {
                entity.TargetConfigs ??= new List<TemplateTargetConfig>();

                foreach (var tc in input.TargetConfigs)
                {
                    var existing = entity.TargetConfigs
                        .FirstOrDefault(x => x.ConfigType == tc.ConfigType);

                    if (existing != null)
                    {
                        existing.WingMale = tc.WingMale;
                        existing.WingFemale = tc.WingFemale;
                        existing.CategoryIds = tc.CategoryIds != null ? JsonSerializer.Serialize(tc.CategoryIds) : null;
                        existing.MandalIds = tc.MandalIds != null ? JsonSerializer.Serialize(tc.MandalIds) : null;
                        existing.FamiliesPairMin = tc.FamiliesPairMin;
                        existing.FamiliesPairMax = tc.FamiliesPairMax;
                        existing.BulkUploadKaryakar = tc.BulkUploadKaryakar;
                        existing.BulkUploadFamily = tc.BulkUploadFamily;
                        existing.BulkUploadAssignment = tc.BulkUploadAssignment;
                        existing.Status = "ACTIVE";
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        entity.TargetConfigs.Add(new TemplateTargetConfig
                        {
                            TenantConfigUucode = Guid.NewGuid(),
                            ConfigType = tc.ConfigType,
                            WingMale = tc.WingMale,
                            WingFemale = tc.WingFemale,
                            CategoryIds = tc.CategoryIds != null ? JsonSerializer.Serialize(tc.CategoryIds) : null,
                            MandalIds = tc.MandalIds != null ? JsonSerializer.Serialize(tc.MandalIds) : null,
                            FamiliesPairMin = tc.FamiliesPairMin,
                            FamiliesPairMax = tc.FamiliesPairMax,
                            BulkUploadKaryakar = tc.BulkUploadKaryakar,
                            BulkUploadFamily = tc.BulkUploadFamily,
                            BulkUploadAssignment = tc.BulkUploadAssignment,
                            Status = "ACTIVE",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                foreach (var db in entity.TargetConfigs)
                {
                    if (!input.TargetConfigs.Any(x => x.ConfigType == db.ConfigType))
                    {
                        db.Status = "INACTIVE";
                        db.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // =====================================================
            // DEPARTMENT CONFIG
            // UNIQUE (template_id, department_id)
            // =====================================================

            if (input.DepartmentConfigs != null)
            {
                entity.DepartmentConfigs ??= new List<TemplateDepartmentConfig>();

                foreach (var dc in input.DepartmentConfigs)
                {
                    var existing = entity.DepartmentConfigs
                        .FirstOrDefault(x => x.DepartmentId == dc.DepartmentId);

                    if (existing != null)
                    {
                        existing.OwnerRoleId = dc.OwnerRoleId;
                        existing.IsPrimary = dc.IsPrimary;
                        existing.Status = "ACTIVE";
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        entity.DepartmentConfigs.Add(new TemplateDepartmentConfig
                        {
                            DepartmentConfigUucode = Guid.NewGuid(),
                            DepartmentId = dc.DepartmentId,
                            OwnerRoleId = dc.OwnerRoleId,
                            IsPrimary = dc.IsPrimary,
                            Status = "ACTIVE",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                foreach (var db in entity.DepartmentConfigs)
                {
                    if (!input.DepartmentConfigs.Any(x => x.DepartmentId == db.DepartmentId))
                    {
                        db.Status = "INACTIVE";
                        db.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // =====================================================
            // TARGET SURVEY
            // UNIQUE (template_id, gss_form_id, config_type)
            // =====================================================

            if (input.TargetSurveys != null)
            {
                entity.TargetSurveys ??= new List<TemplateTargetSurvey>();

                foreach (var s in input.TargetSurveys)
                {
                    var existing = entity.TargetSurveys
                        .FirstOrDefault(x =>
                            x.GssFormId == s.GssFormId &&
                            x.ConfigType == s.ConfigType);

                    if (existing != null)
                    {
                        existing.DepartmentIds = s.DepartmentIds != null ? JsonSerializer.Serialize(s.DepartmentIds) : null;
                        existing.CategoryIds = s.CategoryIds != null ? JsonSerializer.Serialize(s.CategoryIds) : null;
                        existing.IsRequired = s.IsRequired;
                        existing.Status = "ACTIVE";
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        entity.TargetSurveys.Add(new TemplateTargetSurvey
                        {
                            TargetSurveyUucode = Guid.NewGuid(),
                            ConfigType = s.ConfigType,
                            GssFormId = s.GssFormId,
                            DepartmentIds = s.DepartmentIds != null ? JsonSerializer.Serialize(s.DepartmentIds) : null,
                            CategoryIds = s.CategoryIds != null ? JsonSerializer.Serialize(s.CategoryIds) : null,
                            IsRequired = s.IsRequired,
                            Status = "ACTIVE",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                foreach (var db in entity.TargetSurveys)
                {
                    if (!input.TargetSurveys.Any(x =>
                            x.GssFormId == db.GssFormId &&
                            x.ConfigType == db.ConfigType))
                    {
                        db.Status = "INACTIVE";
                        db.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // =====================================================
            // DOCUMENT
            // =====================================================

            if (input.Documents != null)
            {
                entity.Documents ??= new List<TemplateDocument>();

                foreach (var d in input.Documents)
                {
                    var existing = entity.Documents
                        .FirstOrDefault(x => x.DocumentName == d.DocumentName);

                    if (existing != null)
                    {
                        existing.DocumentUrl = d.DocumentUrl;
                        existing.DocumentSfsId = d.DocumentSfsId;
                        existing.DocumentType = d.DocumentType;
                        existing.FileSize = d.FileSize;
                        existing.IsOptional = d.IsOptional;
                        existing.Status = "ACTIVE";
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        entity.Documents.Add(new TemplateDocument
                        {
                            DocumentUucode = Guid.NewGuid(),
                            DocumentName = d.DocumentName,
                            DocumentUrl = d.DocumentUrl,
                            DocumentSfsId = d.DocumentSfsId,
                            DocumentType = d.DocumentType,
                            FileSize = d.FileSize,
                            IsOptional = d.IsOptional,
                            Status = "ACTIVE",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                foreach (var db in entity.Documents)
                {
                    if (!input.Documents.Any(x => x.DocumentName == db.DocumentName))
                    {
                        db.Status = "INACTIVE";
                        db.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            return entity;
        }

        protected override void SetEntityId(Template entity, int id)
        {
            entity.TemplateId = id;
        }

        protected override object GetEntityId(Template entity)
        {
            return entity.TemplateId;
        }

        public async Task<Template> CreateTemplate(
            TemplateInput input,
            [Service] AppDbContext db,
            [Service] IValidator<TemplateInput> validator,
            CancellationToken cancellationToken)
        {
            return await Create(input, db, validator, cancellationToken);
        }

        public async Task<Template> UpdateTemplate(
            int id,
            TemplateInput input,
            [Service] AppDbContext db,
            [Service] IValidator<TemplateInput> validator,
            CancellationToken cancellationToken)
        {
            return await Update(id, input, db, validator, cancellationToken);
        }

        public async Task<bool> DeleteTemplate(
            int id,
            [Service] AppDbContext db,
            CancellationToken cancellationToken)
        {
            return await Delete(id, db, cancellationToken);
        }
        public async Task<Template> PublishTemplate(int id, [Service] AppDbContext db, [Service] IFrequencyService frequencyService, CancellationToken cancellationToken)
        {
            var template = await db.Templates
                .FirstOrDefaultAsync(x => x.TemplateId == id, cancellationToken);

            if (template == null)
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Template not found").SetCode("NOT_FOUND").Build());

            if (template.Status == "PUBLISH")
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Template already published").SetCode("INVALID_OPERATION").Build());

            if (string.IsNullOrEmpty(template.ProjectRepeateFrequencyConfig))
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Frequency configuration not found").SetCode("INVALID_DATA").Build());

            ProjectFrequencyInput? config;

            try
            {
                config = JsonSerializer.Deserialize<ProjectFrequencyInput>(
                    template.ProjectRepeateFrequencyConfig);
            }
            catch
            {
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Invalid frequency configuration").SetCode("JSON_PARSE_ERROR").Build());
            }

            if (config == null)
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Frequency configuration is empty").SetCode("INVALID_DATA").Build());

            var dates = frequencyService.GenerateDates(config);

            if (dates.Count == 0)
                throw new GraphQLException(ErrorBuilder.New().SetMessage("No schedule dates generated").SetCode("INVALID_FREQUENCY").Build());

            template.Status = "PUBLISH";

            foreach (var date in dates)
            {
                db.ProjectSchedules.Add(new ProjectSchedule
                {
                    TemplateId = (int)template.TemplateId,
                    ScheduledDate = date,
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(cancellationToken);

            return template;
        }
    }
}