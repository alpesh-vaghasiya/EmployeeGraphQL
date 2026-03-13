public interface IFrequencyService
{
    List<DateOnly> GenerateDates(ProjectFrequencyInput config);
}