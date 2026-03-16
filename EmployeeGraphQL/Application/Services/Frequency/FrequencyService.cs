namespace Application.Services
{
    public class FrequencyService : IFrequencyService
    {
        public List<DateOnly> GenerateDates(ProjectFrequencyInput config)
        {
            var dates = new List<DateOnly>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (config.Type == "once")
            {
                if (!config.StartDate.HasValue)
                    return dates;

                var d = DateOnly.FromDateTime(config.StartDate.Value);

                if (d >= today)
                    dates.Add(d);

                return dates;
            }

            if (config.Type == "adhoc")
            {
                foreach (var d in config.AdhocDates ?? new List<DateTime>())
                {
                    var date = DateOnly.FromDateTime(d);

                    if (date >= today)
                        dates.Add(date);
                }

                return dates;
            }

            if (config.Type == "repeat")
            {
                if (!config.StartDate.HasValue)
                    return dates;

                if (config.RepeatUnit == "days")
                    return GenerateEveryXDays(config);

                if (config.RepeatUnit == "weeks")
                    return GenerateWeekly(config);

                if (config.RepeatUnit == "months")
                    return GenerateMonthly(config);
            }

            return dates;
        }

        private List<DateOnly> GenerateWeekly(ProjectFrequencyInput config)
        {
            var dates = new List<DateOnly>();

            var current = config.StartDate!.Value;
            var end = config.EndDate ?? current;

            while (current <= end)
            {
                foreach (var day in config.DaysOfWeek ?? new List<string>())
                {
                    var target = Enum.Parse<DayOfWeek>(day, true);

                    var date = GetNextWeekday(DateOnly.FromDateTime(current), target);

                    if (date <= DateOnly.FromDateTime(end))
                        dates.Add(date);
                }

                current = current.AddDays(7 * (config.RepeatEvery ?? 1));
            }

            return dates
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private List<DateOnly> GenerateMonthly(ProjectFrequencyInput config)
        {
            var dates = new List<DateOnly>();

            var current = config.StartDate!.Value;
            var end = config.EndDate ?? current;

            while (current <= end)
            {
                int day = config.DayOfMonth ?? current.Day;

                if (day > DateTime.DaysInMonth(current.Year, current.Month))
                    day = DateTime.DaysInMonth(current.Year, current.Month);

                var date = new DateOnly(current.Year, current.Month, day);

                if (date <= DateOnly.FromDateTime(end))
                    dates.Add(date);

                current = current.AddMonths(config.RepeatEvery ?? 1);
            }

            return dates;
        }

        private List<DateOnly> GenerateEveryXDays(ProjectFrequencyInput config)
        {
            var dates = new List<DateOnly>();

            var current = config.StartDate!.Value;
            var end = config.EndDate ?? current;

            while (current <= end)
            {
                dates.Add(DateOnly.FromDateTime(current));

                current = current.AddDays(config.RepeatEvery ?? 1);
            }

            return dates;
        }

        private DateOnly GetNextWeekday(DateOnly start, DayOfWeek day)
        {
            int diff = ((int)day - (int)start.DayOfWeek + 7) % 7;

            return start.AddDays(diff);
        }
    }
}