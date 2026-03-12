using System;
using System.Collections.Generic;
using Domain.Entities;
using EmployeeGraphQL.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeGraphQL.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Template> Templates { get; set; }
    public DbSet<TemplateTargetConfig> TemplateTargetConfigs { get; set; }
    public DbSet<TemplateDepartmentConfig> TemplateDepartmentConfigs { get; set; }
    public DbSet<TemplateTargetSurvey> TemplateTargetSurveys { get; set; }
    public DbSet<TemplateDocument> TemplateDocuments { get; set; }
    public DbSet<TemplateListView> TemplateListView { get; set; }
    public DbSet<ImportJob> ImportJobs { get; set; }
    public DbSet<ImportRecord> ImportRecords { get; set; }
    public DbSet<ProjectKaryakar> ProjectKaryakars { get; set; }

    public DbSet<EmployeeProject> EmployeeProjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("project_repeat_frequency", new[] { "ONCE", "REPEAT", "ADHOC" })
            .HasPostgresEnum("reminder_frequency", new[] { "ONCE", "REPEAT" })
            .HasPostgresEnum("reminder_trigger", new[] { "AFTER_PROJECT_STARTS" })
            .HasPostgresEnum("reminder_unit", new[] { "DAYS" });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("department_pkey");

            entity.ToTable("department");

            entity.HasIndex(e => e.Name, "department_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Employee>(entity =>
     {
         entity.HasKey(e => e.Id).HasName("employee_pkey");

         entity.ToTable("employee");

         entity.HasIndex(e => e.Email, "employee_email_key").IsUnique();
         entity.HasIndex(e => e.DepartmentId, "idx_employee_department");

         entity.Property(e => e.Id).HasColumnName("id");
         entity.Property(e => e.DepartmentId).HasColumnName("department_id");
         entity.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
         entity.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
         entity.Property(e => e.Salary).HasPrecision(12, 2).HasColumnName("salary");

         entity.HasOne(d => d.Department)
             .WithMany(p => p.Employees)
             .HasForeignKey(d => d.DepartmentId)
             .HasConstraintName("employee_department_id_fkey");

         // ⭐ RELATION WITH EmployeeProject (JOIN TABLE)
         entity.HasMany(e => e.EmployeeProjects)
             .WithOne(ep => ep.Employee)
             .HasForeignKey(ep => ep.EmployeeId);
     });

        modelBuilder.Entity<Project>(entity =>
   {
       entity.ToTable("project");

       entity.HasKey(e => e.ProjectId);

       entity.Property(e => e.ProjectId).HasColumnName("project_id");
       entity.Property(e => e.ProjectUucode).HasColumnName("project_uucode");
       entity.Property(e => e.TemplateId).HasColumnName("template_id");

       entity.Property(e => e.Title).HasColumnName("title");
       entity.Property(e => e.Description).HasColumnName("description");
       entity.Property(e => e.Status).HasColumnName("status");

       entity.Property(e => e.LocationId).HasColumnName("location_id");

       entity.Property(e => e.ProjectStartDate).HasColumnName("project_start_date");
       entity.Property(e => e.ProjectEndDate).HasColumnName("project_end_date");

       entity.Property(e => e.Tags)
           .HasColumnName("tags")
           .HasColumnType("jsonb");

       entity.Property(e => e.ReminderFrequency)
           .HasColumnName("reminder_frequency");

       entity.Property(e => e.ReminderFrequencyConfig)
           .HasColumnName("reminder_frequency_config")
           .HasColumnType("jsonb");

       entity.Property(e => e.CreatedAt).HasColumnName("created_at");
       entity.Property(e => e.CreatedBy).HasColumnName("created_by");
       entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
       entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

       // ⭐ Project → Template FK
       entity.HasOne(p => p.Template)
             .WithMany(t => t.Projects)
             .HasForeignKey(p => p.TemplateId)
             .HasConstraintName("fk_project_template")
             .OnDelete(DeleteBehavior.Restrict);

       // ⭐ Project → Documents
       entity.HasMany(p => p.Documents)
             .WithOne(d => d.Project)
             .HasForeignKey(d => d.ProjectId);
   });

        modelBuilder.Entity<EmployeeProject>(entity =>
 {
     entity.ToTable("employee_project");

     entity.HasKey(e => new { e.EmployeeId, e.ProjectId });

     entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
     entity.Property(e => e.ProjectId).HasColumnName("project_id");

     entity.HasOne(e => e.Employee)
           .WithMany(e => e.EmployeeProjects)
           .HasForeignKey(e => e.EmployeeId);

     entity.HasOne(e => e.Project)
           .WithMany(p => p.EmployeeProjects)
           .HasForeignKey(e => e.ProjectId);
 });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("template_pkey");

            entity.ToTable("template");

            entity.HasIndex(e => e.TemplateUucode, "template_template_uucode_key").IsUnique();

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.AllowedDraftProject)
                .HasColumnType("jsonb")
                .HasColumnName("allowed_draft_project");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CustomDocument)
                .HasDefaultValue(false)
                .HasColumnName("custom_document");
            entity.Property(e => e.CustomReminder)
                .HasDefaultValue(false)
                .HasColumnName("custom_reminder");
            entity.Property(e => e.DefaultProjectCreation)
                .HasDefaultValue(false)
                .HasColumnName("default_project_creation");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.LocationLevelId).HasColumnName("location_level_id");
            entity.Property(e => e.LocationScopeIds)
                .HasColumnType("jsonb")
                .HasColumnName("location_scope_ids");
            entity.Property(e => e.ProjectRepeateFrequencyConfig)
                .HasColumnType("jsonb")
                .HasColumnName("project_repeate_frequency_config");
            entity.Property(e => e.ProjectTypeId).HasColumnName("project_type_id");
            entity.Property(e => e.ReminderFrequencyConfig)
                .HasColumnType("jsonb")
                .HasColumnName("reminder_frequency_config");
            entity.Property(e => e.ReminderValue).HasColumnName("reminder_value");
            entity.Property(e => e.SamparkTypeId).HasColumnName("sampark_type_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'DRAFT'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TemplateUucode).HasColumnName("template_uucode");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("updated_by");

            entity.HasMany(e => e.TargetConfigs)
            .WithOne(e => e.Template)
            .HasForeignKey(e => e.TemplateId);

            entity.HasMany(e => e.DepartmentConfigs)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId);

            entity.HasMany(e => e.TargetSurveys)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId);

            entity.HasMany(e => e.Documents)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId);
        });
        // TEMPLATE TARGET CONFIG
        modelBuilder.Entity<TemplateTargetConfig>(entity =>
 {
     entity.ToTable("template_target_config");

     entity.HasKey(x => x.TargetConfigId);

     entity.Property(x => x.TargetConfigId).HasColumnName("target_config_id");
     entity.Property(x => x.TenantConfigUucode).HasColumnName("tenant_config_uucode");
     entity.Property(x => x.TemplateId).HasColumnName("template_id");
     entity.Property(x => x.ConfigType).HasColumnName("config_type");

     entity.Property(x => x.WingMale).HasColumnName("wing_male");
     entity.Property(x => x.WingFemale).HasColumnName("wing_female");

     entity.Property(x => x.CategoryIds).HasColumnName("category_ids").HasColumnType("jsonb");
     entity.Property(x => x.MandalIds).HasColumnName("mandal_ids").HasColumnType("jsonb");

     // FIXED NAMES
     entity.Property(x => x.FamiliesPairMin).HasColumnName("families_pair_min");
     entity.Property(x => x.FamiliesPairMax).HasColumnName("families_pair_max");

     entity.Property(x => x.BulkUploadKaryakar).HasColumnName("bulk_upload_karyakar");
     entity.Property(x => x.BulkUploadFamily).HasColumnName("bulk_upload_family");
     entity.Property(x => x.BulkUploadAssignment).HasColumnName("bulk_upload_assignment");

     entity.Property(x => x.CreatedAt).HasColumnName("created_at");
     entity.Property(x => x.CreatedBy).HasColumnName("created_by");
     entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
     entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");

     entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");

     entity.HasIndex(x => new { x.TemplateId, x.ConfigType })
           .IsUnique();
     entity.HasOne(x => x.Template)
 .WithMany(t => t.TargetConfigs)
 .HasForeignKey(x => x.TemplateId)
 .HasConstraintName("template_target_config_template_id_fkey");
 });

        // TEMPLATE DEPARTMENT CONFIG
        modelBuilder.Entity<TemplateDepartmentConfig>(entity =>
        {
            entity.ToTable("template_department_config");

            entity.HasKey(x => x.DepartmentConfigId);

            entity.Property(x => x.DepartmentConfigId).HasColumnName("department_config_id");
            entity.Property(x => x.DepartmentConfigUucode).HasColumnName("department_config_uucode");
            entity.Property(x => x.TemplateId).HasColumnName("template_id");
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.OwnerRoleId).HasColumnName("owner_role_id");
            entity.Property(x => x.IsPrimary).HasColumnName("is_primary");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");


            entity.HasIndex(x => new { x.TemplateId, x.DepartmentId }).IsUnique();
            entity.HasOne(x => x.Template)
    .WithMany(t => t.DepartmentConfigs)
    .HasForeignKey(x => x.TemplateId);
        });

        // TEMPLATE TARGET SURVEY
        modelBuilder.Entity<TemplateTargetSurvey>(entity =>
        {
            entity.ToTable("template_target_survey");

            entity.HasKey(x => x.TargetSurveyId);

            entity.Property(x => x.TargetSurveyId).HasColumnName("target_survey_id");
            entity.Property(x => x.TargetSurveyUucode).HasColumnName("target_survey_uucode");
            entity.Property(x => x.TemplateId).HasColumnName("template_id");
            entity.Property(x => x.ConfigType).HasColumnName("config_type");
            entity.Property(x => x.GssFormId).HasColumnName("gss_form_id");

            entity.Property(x => x.DepartmentIds).HasColumnName("department_ids").HasColumnType("jsonb");
            entity.Property(x => x.CategoryIds).HasColumnName("category_ids").HasColumnType("jsonb");

            entity.Property(x => x.IsRequired).HasColumnName("is_required");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");

            entity.HasIndex(x => new { x.TemplateId, x.GssFormId, x.ConfigType })
                  .IsUnique();
            entity.HasOne(x => x.Template)
.WithMany(t => t.TargetSurveys)
.HasForeignKey(x => x.TemplateId);
        });

        // TEMPLATE DOCUMENT
        modelBuilder.Entity<TemplateDocument>(entity =>
        {
            entity.ToTable("template_document");

            entity.HasKey(x => x.DocumentId);

            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.DocumentUucode).HasColumnName("document_uucode");
            entity.Property(x => x.TemplateId).HasColumnName("template_id");

            entity.Property(x => x.DocumentName).HasColumnName("document_name");
            entity.Property(x => x.DocumentUrl).HasColumnName("document_url");
            entity.Property(x => x.DocumentSfsId).HasColumnName("document_sfs_id");
            entity.Property(x => x.DocumentType).HasColumnName("document_type");
            entity.Property(x => x.FileSize).HasColumnName("file_size");
            entity.Property(x => x.IsOptional).HasColumnName("is_optional");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");


            entity.HasIndex(x => x.TemplateId);
            entity.HasOne(x => x.Template)
    .WithMany(t => t.Documents)
    .HasForeignKey(x => x.TemplateId);
        });

        modelBuilder.Entity<ProjectKaryakar>(entity =>
 {
     entity.ToTable("project_karyakar");

     entity.HasKey(e => e.ProjectKaryakarId)
           .HasName("project_karyakar_pkey");

     entity.Property(e => e.ProjectKaryakarId)
           .HasColumnName("project_karyakar_id");

     entity.Property(e => e.ProjectKaryakarUucode)
           .HasColumnName("project_karyakar_uucode");

     entity.Property(e => e.ProjectId)
           .HasColumnName("project_id");

     entity.Property(e => e.KaryakarPersonId)
           .HasColumnName("karyakar_person_id");

     entity.Property(e => e.CategoryId).HasColumnName("category_id");
     entity.Property(e => e.MandalId).HasColumnName("mandal_id");
     entity.Property(e => e.DepartmentId).HasColumnName("department_id");

     entity.Property(e => e.Status).HasColumnName("status");

     entity.Property(e => e.CreatedAt).HasColumnName("created_at");
     entity.Property(e => e.CreatedBy).HasColumnName("created_by");
     entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
     entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

     entity.HasIndex(e => e.ProjectId)
           .HasDatabaseName("idx_project_karyakar_project");

     entity.HasIndex(e => e.KaryakarPersonId)
           .HasDatabaseName("idx_project_karyakar_karyakar");

     entity.HasIndex(e => new { e.ProjectId, e.KaryakarPersonId })
           .IsUnique()
           .HasDatabaseName("project_karyakar_project_id_karyakar_person_id_key");

     entity.HasOne(p => p.Project)
           .WithMany(p => p.Karyakars)
           .HasForeignKey(p => p.ProjectId);
 });

        modelBuilder.Entity<ProjectKaryakarPair>(entity =>
        {
            entity.ToTable("project_karyakar_pair");

            entity.HasKey(e => e.KaryakarPairId);

            entity.Property(e => e.KaryakarPairId).HasColumnName("karyakar_pair_id");
            entity.Property(e => e.KaryakarPairUucode).HasColumnName("karyakar_pair_uucode");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.Property(e => e.PrimaryKaryakarPersonId).HasColumnName("primary_karyakar_person_id");
            entity.Property(e => e.SecondaryKaryakarPersonId).HasColumnName("secondary_karyakar_person_id");

            entity.Property(e => e.PairType).HasColumnName("pair_type");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            modelBuilder.Entity<ProjectKaryakarPair>()
    .HasOne(k => k.Project)
    .WithMany(p => p.KaryakarPairs)
    .HasForeignKey(k => k.ProjectId);
        });
        modelBuilder.Entity<ProjectFamily>()
    .HasOne(f => f.AssignedPair)
    .WithMany(p => p.Families)
    .HasForeignKey(f => f.AssignedKaryakarPairId);

        modelBuilder.Entity<ProjectFamily>(entity =>
{
    entity.ToTable("project_family");

    entity.HasKey(e => e.ProjectFamilyId);

    entity.Property(e => e.ProjectFamilyId).HasColumnName("project_family_id");
    entity.Property(e => e.ProjectFamilyUucode).HasColumnName("project_family_uucode");
    entity.Property(e => e.ProjectId).HasColumnName("project_id");

    entity.Property(e => e.PrimaryMemberName).HasColumnName("primary_member_name");
    entity.Property(e => e.PrimaryPersonId).HasColumnName("primary_person_id");

    entity.Property(e => e.CategoryId).HasColumnName("category_id");
    entity.Property(e => e.MandalId).HasColumnName("mandal_id");
    entity.Property(e => e.DepartmentId).HasColumnName("department_id");

    entity.Property(e => e.AssignedKaryakarPairId).HasColumnName("assigned_karyakar_pair_id");

    entity.Property(e => e.Status).HasColumnName("status");

    entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    entity.Property(e => e.CreatedBy).HasColumnName("created_by");
    entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
    modelBuilder.Entity<ProjectFamily>()
    .HasOne(f => f.Project)
    .WithMany(p => p.Families)
    .HasForeignKey(f => f.ProjectId);
});

        modelBuilder.Entity<ProjectDocument>(entity =>
 {
     entity.ToTable("project_document");

     entity.HasKey(e => e.ProjectDocumentId);

     entity.Property(e => e.ProjectDocumentId).HasColumnName("project_document_id");
     entity.Property(e => e.ProjectId).HasColumnName("project_id");

     entity.Property(e => e.DocumentName).HasColumnName("document_name");
     entity.Property(e => e.DocumentUrl).HasColumnName("document_url");
     entity.Property(e => e.DocumentSfsId).HasColumnName("document_sfs_id");
     entity.Property(e => e.DocumentType).HasColumnName("document_type");
     entity.Property(e => e.FileSize).HasColumnName("file_size");
     entity.Property(e => e.IsCustomUpload).HasColumnName("is_custom_upload");
     entity.Property(e => e.CreatedAt).HasColumnName("created_at");
     entity.Property(e => e.CreatedBy).HasColumnName("created_by");
     entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
     entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

     // ⭐ CORRECT RELATIONSHIP
     entity.HasOne(d => d.Project)
           .WithMany(p => p.Documents)
           .HasForeignKey(d => d.ProjectId)
           .HasConstraintName("project_document_project_id_fkey");
 });
        modelBuilder.Entity<ImportJob>(entity =>
{
    entity.ToTable("import_job");

    entity.HasKey(e => e.ImportJobId)
        .HasName("import_job_pkey");

    entity.Property(e => e.ImportJobId)
        .HasColumnName("import_job_id")
        .HasMaxLength(100);

    entity.Property(e => e.ProjectId).HasColumnName("project_id");
    entity.Property(e => e.ImportType).HasColumnName("import_type");
    entity.Property(e => e.ProcessingMode).HasColumnName("processing_mode");
    entity.Property(e => e.FileUrl).HasColumnName("file_url");
    entity.Property(e => e.FileName).HasColumnName("file_name");
    entity.Property(e => e.FileSize).HasColumnName("file_size");
    entity.Property(e => e.TotalRecords).HasColumnName("total_records");
    entity.Property(e => e.ValidRecords).HasColumnName("valid_records");
    entity.Property(e => e.InvalidRecords).HasColumnName("invalid_records");
    entity.Property(e => e.ImportedRecords).HasColumnName("imported_records");
    entity.Property(e => e.Status).HasColumnName("status");
    entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
    entity.Property(e => e.StartedAt).HasColumnName("started_at");
    entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
    entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    entity.Property(e => e.CreatedBy).HasColumnName("created_by");
    entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

    entity.HasMany(e => e.ImportRecords)
        .WithOne(r => r.ImportJob)
        .HasForeignKey(r => r.ImportJobId)
        .HasConstraintName("import_record_import_job_id_fkey");
});

        modelBuilder.Entity<ImportRecord>(entity =>
{
    entity.ToTable("import_record");

    entity.HasKey(e => e.ImportRecordId)
        .HasName("import_record_pkey");

    entity.Property(e => e.ImportRecordId).HasColumnName("import_record_id");
    entity.Property(e => e.ImportRecordUuCode).HasColumnName("import_record_uu_code");

    entity.Property(e => e.ImportJobId)
        .HasColumnName("import_job_id")
        .HasMaxLength(100);

    entity.Property(e => e.RowNumber).HasColumnName("row_number");

    entity.Property(e => e.RecordData)
        .HasColumnName("record_data")
        .HasColumnType("jsonb");

    entity.Property(e => e.IsValid).HasColumnName("is_valid");
    entity.Property(e => e.ValidationErrors).HasColumnName("validation_errors");
    entity.Property(e => e.IsImported).HasColumnName("is_imported");
    entity.Property(e => e.ImportedAt).HasColumnName("imported_at");
    entity.Property(e => e.ImportedEntityType).HasColumnName("imported_entity_type");
    entity.Property(e => e.ImportedEntityId).HasColumnName("imported_entity_id");
    entity.Property(e => e.CreatedAt).HasColumnName("created_at");
    entity.Property(e => e.CreatedBy).HasColumnName("created_by");
    entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

    // Proper FK
    entity.HasOne(e => e.ImportJob)
        .WithMany(j => j.ImportRecords)
        .HasForeignKey(e => e.ImportJobId)
        .HasConstraintName("import_record_import_job_id_fkey")
        .OnDelete(DeleteBehavior.Cascade);
});

        modelBuilder.Entity<TemplateListView>()
     .HasNoKey()
     .ToView("vw_template_list");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.TemplateId)
            .HasColumnName("template_id");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.Title)
            .HasColumnName("title");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.Description)
            .HasColumnName("description");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.ProjectType)
            .HasColumnName("projecttype");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.SamparkType)
            .HasColumnName("samparktype");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.LocationScope)
            .HasColumnName("locationscope");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.Departments)
            .HasColumnName("departments")
            .HasColumnType("text[]");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.StartDate)
            .HasColumnName("start_date");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.EndDate)
            .HasColumnName("end_date");

        modelBuilder.Entity<TemplateListView>()
            .Property(x => x.Status)
            .HasColumnName("status");
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
