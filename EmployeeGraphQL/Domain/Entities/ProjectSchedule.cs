using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("project_schedule")]
    public class ProjectSchedule
    {
        [Key]
        [Column("project_schedule_id")]
        public int ProjectScheduleId { get; set; }

        [Column("template_id")]
        public int TemplateId { get; set; }

        [Column("scheduled_date")]
        public DateOnly ScheduledDate { get; set; }

        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}