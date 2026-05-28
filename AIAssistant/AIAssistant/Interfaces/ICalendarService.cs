namespace AIAssistant.Interfaces
{
    public interface ICalendarService
    {
        Task<IEnumerable<string>> GetEventsForDateAsync(DateOnly date);
    }
}
