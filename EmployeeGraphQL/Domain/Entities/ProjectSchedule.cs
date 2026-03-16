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

        [Column("project_id")]
        public long? ProjectId { get; set; }   // FIX HERE

        [Column("scheduled_date")]
        public DateOnly ScheduledDate { get; set; }

        [Column("schedule_type")]
        public string ScheduleType { get; set; } = "PROJECT";

        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}