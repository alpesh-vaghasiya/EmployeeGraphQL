using System.Text.Json;
using EmployeeGraphQL.Domain.Entities;

public class ReminderService
{
    public List<DateTime> GenerateDates(Project project)
    {
        var reminders = new List<DateTime>();

        if (string.IsNullOrEmpty(project.ReminderFrequencyConfig))
            return reminders;

        var config = JsonSerializer.Deserialize<ReminderConfigInput>(
            project.ReminderFrequencyConfig);

        if (config == null)
            return reminders;

        var start = project.ProjectStartDate.AddDays(project.Template?.ReminderValue ?? 0);

        var time = TimeSpan.Parse(config.Time);

        var first = start.Date.Add(time);

        if (config.Frequency == "ONCE")
        {
            reminders.Add(first);
            return reminders;
        }

        if (config.Frequency == "REPEAT")
        {
            var current = first;

            while (current <= project.ProjectEndDate)
            {
                reminders.Add(current);

                switch (config.RepeatUnit?.ToUpper())
                {
                    case "DAYS":
                        current = current.AddDays(config.RepeatEvery ?? 1);
                        break;

                    case "WEEKS":
                        current = current.AddDays((config.RepeatEvery ?? 1) * 7);
                        break;

                    case "MONTHS":
                        current = current.AddMonths(config.RepeatEvery ?? 1);
                        break;

                    default:
                        current = current.AddDays(config.RepeatEvery ?? 1);
                        break;
                }
            }
        }

        return reminders;
    }
}