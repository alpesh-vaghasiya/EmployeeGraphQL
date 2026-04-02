using FluentValidation;

public class ProjectFrequencyValidator : AbstractValidator<ProjectFrequencyInput>
{
    public ProjectFrequencyValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t == "once" || t == "repeat" || t == "adhoc")
            .WithMessage("Type must be once | repeat | adhoc");

        RuleFor(x => x.StartDate).NotNull();
        RuleFor(x => x.EndDate)
            .NotNull()
            .GreaterThan(x => x.StartDate);

        // ===============================
        // ONCE
        // ===============================
        When(x => x.Type == "once", () =>
        {
            RuleFor(x => x)
                .Must(x =>
                {
                    var totalDays = FrequencyValidationHelper.GetTotalDays(
                        x.StartDate!.Value,
                        x.EndDate!.Value);

                    return x.MaxDurationDays <= totalDays;
                })
                .WithMessage("MaxDurationDays cannot exceed total duration days");

            RuleFor(x => x)
                .Must(x =>
                {
                    var totalDays = FrequencyValidationHelper.GetTotalDays(
                        x.StartDate!.Value,
                        x.EndDate!.Value);

                    return totalDays >= x.MinDurationDays;
                })
                .WithMessage("Total duration days must be >= MinDurationDays");

            RuleFor(x => x)
                .Must(x => x.MinDurationDays <= x.MaxDurationDays)
                .WithMessage("MinDurationDays must be <= MaxDurationDays");
        });

        // ===============================
        // REPEAT
        // ===============================
        When(x => x.Type == "repeat", () =>
        {
            RuleFor(x => x.RepeatEvery)
                .GreaterThan(0);

            RuleFor(x => x.RepeatUnit)
                .Must(u => u == "days" || u == "weeks" || u == "months");

            // repeat every days
            When(x => x.RepeatUnit == "days", () =>
            {
                RuleFor(x => x)
                    .Must(x =>
                    {
                        var totalDays = FrequencyValidationHelper.GetTotalDays(
                            x.StartDate!.Value,
                            x.EndDate!.Value);

                        return x.RepeatEvery <= totalDays;
                    })
                    .WithMessage("RepeatEvery days cannot exceed duration days");
            });

            // repeat every weeks
            When(x => x.RepeatUnit == "weeks", () =>
            {
                RuleFor(x => x)
                    .Must(x =>
                    {
                        var totalWeeks = FrequencyValidationHelper.GetTotalWeeks(
                            x.StartDate!.Value,
                            x.EndDate!.Value);

                        return x.RepeatEvery <= totalWeeks;
                    })
                    .WithMessage("RepeatEvery weeks cannot exceed duration");
            });

            // repeat every months
            When(x => x.RepeatUnit == "months", () =>
            {
                RuleFor(x => x.DayOfMonth)
                    .InclusiveBetween(1, 31)
                    .WithMessage("StartOn must be between 1 to 31");

                RuleFor(x => x)
                    .Must(x =>
                    {
                        var totalMonths = FrequencyValidationHelper.GetTotalMonths(
                            x.StartDate!.Value,
                            x.EndDate!.Value);

                        return x.RepeatEvery <= totalMonths;
                    })
                    .WithMessage("RepeatEvery months cannot exceed duration");
            });

            // min/max duration check
            RuleFor(x => x)
                .Must(x => x.MinDurationDays <= x.MaxDurationDays)
                .WithMessage("MinDurationDays must be <= MaxDurationDays");

            // max duration check
            RuleFor(x => x)
                .Must(x =>
                {
                    var totalDays = FrequencyValidationHelper.GetTotalDays(
                        x.StartDate!.Value,
                        x.EndDate!.Value);

                    return x.MaxDurationDays <= totalDays;
                })
                .WithMessage("MaxDurationDays cannot exceed total duration days");
        });

        // ===============================
        // ADHOC
        // ===============================
        When(x => x.Type == "adhoc", () =>
        {
            RuleFor(x => x.CreateProjectTimes)
                .GreaterThan(0);

            RuleFor(x => x)
                .Must(x =>
                    x.AdhocDates == null ||
                    x.AdhocDates.Count <= (x.CreateProjectTimes ?? int.MaxValue))
                .WithMessage("AdhocDates count cannot exceed CreateProjectTimes");

            RuleFor(x => x)
                .Must(x =>
                {
                    var totalDays = FrequencyValidationHelper.GetTotalDays(
                        x.StartDate!.Value,
                        x.EndDate!.Value);

                    return x.MaxDurationDays <= totalDays;
                })
                .WithMessage("MaxDurationDays cannot exceed total duration days");

            RuleFor(x => x)
                .Must(x =>
                    x.AdhocDates == null ||
                    x.AdhocDates.All(d =>
                        d.Date >= x.StartDate!.Value.Date &&
                        d.Date <= x.EndDate!.Value.Date))
                .WithMessage("Adhoc dates must be within project duration");
        });
    }
    public static class FrequencyValidationHelper
    {
        public static int GetTotalDays(DateTime start, DateTime end)
            => (end.Date - start.Date).Days + 1;

        public static int GetTotalWeeks(DateTime start, DateTime end)
            => (int)Math.Ceiling(GetTotalDays(start, end) / 7.0);

        public static int GetTotalMonths(DateTime start, DateTime end)
            => ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;
    }
}