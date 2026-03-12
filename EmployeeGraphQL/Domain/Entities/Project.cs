using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;

namespace EmployeeGraphQL.Domain.Entities;

public partial class Project
{
    public long ProjectId { get; set; }                      // project_id
    public Guid ProjectUucode { get; set; }                 // project_uucode

    public long TemplateId { get; set; }                    // template_id

    public string Title { get; set; }                       // title
    public string? Description { get; set; }                // description
    public string Status { get; set; }                      // status

    public string? LocationId { get; set; }                 // location_id

    public DateTime ProjectStartDate { get; set; }          // project_start_date
    public DateTime ProjectEndDate { get; set; }            // project_end_date

    public string? Tags { get; set; }                       // stored as JSONB
    public string? ReminderFrequency { get; set; }          // reminder_frequency
    public string? ReminderFrequencyConfig { get; set; }    // reminder_frequency_config JSONB

    public DateTime CreatedAt { get; set; }                 // created_at
    public string? CreatedBy { get; set; }                  // created_by
    public DateTime? UpdatedAt { get; set; }                // updated_at
    public string? UpdatedBy { get; set; }

    // ⭐ Navigation Properties
    public ICollection<ProjectFamily> Families { get; set; } = new List<ProjectFamily>();
    public ICollection<ProjectKaryakar> Karyakars { get; set; } = new List<ProjectKaryakar>();
    public ICollection<ProjectKaryakarPair> KaryakarPairs { get; set; } = new List<ProjectKaryakarPair>();
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
    public ICollection<EmployeeProject>? EmployeeProjects { get; set; }
}
