// using System.Text.Json;
// using EmployeeGraphQL.Domain.Entities;
// using EmployeeGraphQL.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;

// namespace Api.GraphQL;

// [ExtendObjectType(typeof(Mutation))]
// public class TemplateMutation
// {
//     public async Task<Template> CreateTemplate(
//        TemplateFullInput input,
//        [Service] AppDbContext context)
//     {
//         // 1️⃣ Create base template
//         var t = input.Template;

//         var template = new Template
//         {
//             TemplateUucode = Guid.NewGuid(),
//             Title = t.Title,
//             Description = t.Description,
//             Status = "DRAFT",   // default status
//             ProjectTypeId = t.ProjectTypeId,
//             SamparkTypeId = t.SamparkTypeId,
//             LocationScopeIds = t.LocationScopeIds,
//             LocationLevelId = t.LocationLevelId,
//             AllowedDraftProject = t.AllowedDraftProject,    // already JSON string
//             DefaultProjectCreation = t.DefaultProjectCreation,
//             StartDate = t.StartDate,
//             EndDate = t.EndDate,
//             ProjectRepeateFrequencyConfig = t.ProjectRepeateFrequencyConfig,
//             ReminderValue = t.ReminderValue,
//             ReminderFrequencyConfig = t.ReminderFrequencyConfig,
//             CustomReminder = t.CustomReminder,
//             CustomDocument = t.CustomDocument,
//             CreatedAt = DateTime.Now
//         };

//         context.Templates.Add(template);
//         await context.SaveChangesAsync();  // Needed to generate template_id

//         long templateId = template.TemplateId;

//         // 2️⃣ Add Target Configs
//         if (input.TargetConfigs != null)
//         {
//             foreach (var tc in input.TargetConfigs)
//             {
//                 context.TemplateTargetConfigs.Add(new TemplateTargetConfig
//                 {
//                     TemplateId = templateId,
//                     TenantConfigUucode = Guid.NewGuid(),
//                     ConfigType = tc.ConfigType,
//                     WingMale = tc.WingMale,
//                     WingFemale = tc.WingFemale,
//                     CategoryIds = tc.CategoryIds != null ? JsonSerializer.Serialize(tc.CategoryIds) : null,
//                     MandalIds = tc.MandalIds != null ? JsonSerializer.Serialize(tc.MandalIds) : null,
//                     FamiliesPairMin = tc.FamiliesPairMin,
//                     FamiliesPairMax = tc.FamiliesPairMax,
//                     BulkUploadKaryakar = tc.BulkUploadKaryakar,
//                     BulkUploadFamily = tc.BulkUploadFamily,
//                     BulkUploadAssignment = tc.BulkUploadAssignment,
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // 3️⃣ Add Department Configs
//         if (input.DepartmentConfigs != null)
//         {
//             foreach (var dc in input.DepartmentConfigs)
//             {
//                 context.TemplateDepartmentConfigs.Add(new TemplateDepartmentConfig
//                 {
//                     TemplateId = templateId,
//                     DepartmentConfigUucode = Guid.NewGuid(),
//                     DepartmentId = dc.DepartmentId,
//                     OwnerRoleId = dc.OwnerRoleId,
//                     IsPrimary = dc.IsPrimary,
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // 4️⃣ Add Surveys
//         if (input.TargetSurveys != null)
//         {
//             foreach (var s in input.TargetSurveys)
//             {
//                 context.TemplateTargetSurveys.Add(new TemplateTargetSurvey
//                 {
//                     TemplateId = templateId,
//                     TargetSurveyUucode = Guid.NewGuid(),
//                     ConfigType = s.ConfigType,
//                     GssFormId = s.GssFormId,
//                     DepartmentIds = s.DepartmentIds != null ? JsonSerializer.Serialize(s.DepartmentIds) : null,
//                     CategoryIds = s.CategoryIds != null ? JsonSerializer.Serialize(s.CategoryIds) : null,
//                     IsRequired = s.IsRequired,
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // 5️⃣ Add Documents
//         if (input.Documents != null)
//         {
//             foreach (var d in input.Documents)
//             {
//                 context.TemplateDocuments.Add(new TemplateDocument
//                 {
//                     TemplateId = templateId,
//                     DocumentUucode = Guid.NewGuid(),
//                     DocumentName = d.DocumentName,
//                     DocumentUrl = d.DocumentUrl,
//                     DocumentSfsId = d.DocumentSfsId,
//                     DocumentType = d.DocumentType,
//                     FileSize = d.FileSize,
//                     IsOptional = d.IsOptional,
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         await context.SaveChangesAsync();

//         return template;
//     }

//     public async Task<Template?> UpdateTemplate(TemplateFullUpdateInput input, [Service] AppDbContext context)
//     {
//         var t = input.Template;

//         // 1️⃣ Get Template
//         var template = await context.Templates
//             .FirstOrDefaultAsync(x => x.TemplateId == t.TemplateId);

//         if (template == null)
//             throw new Exception("Template not found.");

//         // 2️⃣ Business Rule: Only DRAFT can update
//         if (!string.Equals(template.Status, "DRAFT", StringComparison.OrdinalIgnoreCase))
//             throw new Exception("Template can only be updated when status is DRAFT.");

//         // 3️⃣ Update Base Template
//         template.Title = t.Title ?? template.Title;
//         template.Description = t.Description ?? template.Description;
//         template.ProjectTypeId = t.ProjectTypeId ?? template.ProjectTypeId;
//         template.SamparkTypeId = t.SamparkTypeId ?? template.SamparkTypeId;
//         template.LocationScopeIds = t.LocationScopeIds ?? template.LocationScopeIds;
//         template.LocationLevelId = t.LocationLevelId ?? template.LocationLevelId;
//         template.AllowedDraftProject = t.AllowedDraftProject ?? template.AllowedDraftProject;
//         template.DefaultProjectCreation = t.DefaultProjectCreation ?? template.DefaultProjectCreation;
//         template.StartDate = t.StartDate ?? template.StartDate;
//         template.EndDate = t.EndDate ?? template.EndDate;
//         template.ProjectRepeateFrequencyConfig = t.ProjectRepeateFrequencyConfig ?? template.ProjectRepeateFrequencyConfig;
//         template.ReminderValue = t.ReminderValue ?? template.ReminderValue;
//         template.ReminderFrequencyConfig = t.ReminderFrequencyConfig ?? template.ReminderFrequencyConfig;
//         template.CustomReminder = t.CustomReminder ?? template.CustomReminder;
//         template.CustomDocument = t.CustomDocument ?? template.CustomDocument;
//         template.UpdatedAt = DateTime.Now;

//         long templateId = t.TemplateId;

//         // **************************************************
//         // 4️⃣ SMART UPDATE – TARGET CONFIGS
//         // **************************************************
//         var dbTC = await context.TemplateTargetConfigs.Where(x => x.TemplateId == templateId).ToListAsync();

//         List<TargetConfigUpdateInput> inputTC = input.TargetConfigs ?? new List<TargetConfigUpdateInput>();

//         foreach (var db in dbTC)
//         {
//             var match = inputTC.FirstOrDefault(x => x.ConfigType == db.ConfigType);

//             if (match == null)
//             {
//                 db.Status = "INACTIVE";
//                 db.UpdatedAt = DateTime.UtcNow;
//                 continue;
//             }

//             db.WingMale = match.WingMale;
//             db.WingFemale = match.WingFemale;
//             db.CategoryIds = match.CategoryIds != null ? JsonSerializer.Serialize(match.CategoryIds) : null;
//             db.MandalIds = match.MandalIds != null ? JsonSerializer.Serialize(match.MandalIds) : null;
//             db.FamiliesPairMin = match.FamiliesPairMin;
//             db.FamiliesPairMax = match.FamiliesPairMax;
//             db.BulkUploadKaryakar = match.BulkUploadKaryakar;
//             db.BulkUploadFamily = match.BulkUploadFamily;
//             db.BulkUploadAssignment = match.BulkUploadAssignment;
//             db.Status = "ACTIVE";
//             db.UpdatedAt = DateTime.UtcNow;
//         }

//         // Add new
//         foreach (var tc in inputTC)
//         {
//             if (!dbTC.Any(x => x.ConfigType == tc.ConfigType))
//             {
//                 context.TemplateTargetConfigs.Add(new TemplateTargetConfig
//                 {
//                     TemplateId = templateId,
//                     TenantConfigUucode = Guid.NewGuid(),
//                     ConfigType = tc.ConfigType,
//                     WingMale = tc.WingMale,
//                     WingFemale = tc.WingFemale,
//                     CategoryIds = tc.CategoryIds != null ? JsonSerializer.Serialize(tc.CategoryIds) : null,
//                     MandalIds = tc.MandalIds != null ? JsonSerializer.Serialize(tc.MandalIds) : null,
//                     FamiliesPairMin = tc.FamiliesPairMin,
//                     FamiliesPairMax = tc.FamiliesPairMax,
//                     BulkUploadKaryakar = tc.BulkUploadKaryakar,
//                     BulkUploadFamily = tc.BulkUploadFamily,
//                     BulkUploadAssignment = tc.BulkUploadAssignment,
//                     Status = "ACTIVE",
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // **************************************************
//         // 5️⃣ SMART UPDATE – DEPARTMENT CONFIG
//         // **************************************************
//         var dbDept = await context.TemplateDepartmentConfigs
//             .Where(x => x.TemplateId == templateId)
//             .ToListAsync();

//         var inputDept = input.DepartmentConfigs ?? new List<DepartmentConfigUpdateInput>();

//         foreach (var db in dbDept)
//         {
//             var match = inputDept.FirstOrDefault(x => x.DepartmentId == db.DepartmentId);

//             if (match == null)
//             {
//                 db.Status = "INACTIVE";
//                 db.UpdatedAt = DateTime.UtcNow;
//                 continue;
//             }

//             db.OwnerRoleId = match.OwnerRoleId;
//             db.IsPrimary = match.IsPrimary;
//             db.Status = "ACTIVE";
//             db.UpdatedAt = DateTime.UtcNow;
//         }

//         foreach (var dc in inputDept)
//         {
//             if (!dbDept.Any(x => x.DepartmentId == dc.DepartmentId))
//             {
//                 context.TemplateDepartmentConfigs.Add(new TemplateDepartmentConfig
//                 {
//                     TemplateId = templateId,
//                     DepartmentConfigUucode = Guid.NewGuid(),
//                     DepartmentId = dc.DepartmentId,
//                     OwnerRoleId = dc.OwnerRoleId,
//                     IsPrimary = dc.IsPrimary,
//                     Status = "ACTIVE",
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // **************************************************
//         // 6️⃣ SMART UPDATE – TARGET SURVEY
//         // **************************************************
//         var dbSurvey = await context.TemplateTargetSurveys
//             .Where(x => x.TemplateId == templateId)
//             .ToListAsync();

//         var inputSurvey = input.TargetSurveys ?? new List<TargetSurveyUpdateInput>();

//         foreach (var db in dbSurvey)
//         {
//             var match = inputSurvey
//                 .FirstOrDefault(x => x.GssFormId == db.GssFormId && x.ConfigType == db.ConfigType);

//             if (match == null)
//             {
//                 db.Status = "INACTIVE";
//                 db.UpdatedAt = DateTime.UtcNow;
//                 continue;
//             }

//             db.CategoryIds = match.CategoryIds != null ? JsonSerializer.Serialize(match.CategoryIds) : null;
//             db.DepartmentIds = match.DepartmentIds != null ? JsonSerializer.Serialize(match.DepartmentIds) : null;
//             db.IsRequired = match.IsRequired;
//             db.Status = "ACTIVE";
//             db.UpdatedAt = DateTime.UtcNow;
//         }

//         foreach (var s in inputSurvey)
//         {
//             if (!dbSurvey.Any(x => x.GssFormId == s.GssFormId && x.ConfigType == s.ConfigType))
//             {
//                 context.TemplateTargetSurveys.Add(new TemplateTargetSurvey
//                 {
//                     TemplateId = templateId,
//                     TargetSurveyUucode = Guid.NewGuid(),
//                     ConfigType = s.ConfigType,
//                     GssFormId = s.GssFormId,
//                     DepartmentIds = s.DepartmentIds != null ? JsonSerializer.Serialize(s.DepartmentIds) : null,
//                     CategoryIds = s.CategoryIds != null ? JsonSerializer.Serialize(s.CategoryIds) : null,
//                     IsRequired = s.IsRequired,
//                     Status = "ACTIVE",
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         // **************************************************
//         // 7️⃣ SMART UPDATE – DOCUMENTS
//         // **************************************************
//         var dbDocs = await context.TemplateDocuments
//             .Where(x => x.TemplateId == templateId)
//             .ToListAsync();

//         var inputDocs = input.Documents ?? new List<DocumentUpdateInput>();

//         foreach (var db in dbDocs)
//         {
//             var match = inputDocs.FirstOrDefault(x => x.DocumentName == db.DocumentName);

//             if (match == null)
//             {
//                 db.Status = "INACTIVE";
//                 db.UpdatedAt = DateTime.UtcNow;
//                 continue;
//             }

//             db.DocumentUrl = match.DocumentUrl;
//             db.DocumentSfsId = match.DocumentSfsId;
//             db.DocumentType = match.DocumentType;
//             db.FileSize = match.FileSize;
//             db.IsOptional = match.IsOptional;
//             db.Status = "ACTIVE";
//             db.UpdatedAt = DateTime.UtcNow;
//         }

//         foreach (var d in inputDocs)
//         {
//             if (!dbDocs.Any(x => x.DocumentName == d.DocumentName))
//             {
//                 context.TemplateDocuments.Add(new TemplateDocument
//                 {
//                     TemplateId = templateId,
//                     DocumentUucode = Guid.NewGuid(),
//                     DocumentName = d.DocumentName,
//                     DocumentUrl = d.DocumentUrl,
//                     DocumentSfsId = d.DocumentSfsId,
//                     DocumentType = d.DocumentType,
//                     FileSize = d.FileSize,
//                     IsOptional = d.IsOptional,
//                     Status = "ACTIVE",
//                     CreatedAt = DateTime.UtcNow
//                 });
//             }
//         }

//         await context.SaveChangesAsync();
//         return template;
//     }

//     public async Task<bool> DeleteTemplate(
//         long templateId,
//         [Service] AppDbContext context)
//     {
//         var entity = await context.Templates
//             .FirstOrDefaultAsync(x => x.TemplateId == templateId);

//         if (entity == null)
//             return false;

//         context.Templates.Remove(entity);
//         await context.SaveChangesAsync();
//         return true;
//     }
// }