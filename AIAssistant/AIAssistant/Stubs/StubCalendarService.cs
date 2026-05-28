using AIAssistant.Interfaces;

namespace AIAssistant.Stubs
{
    public class StubCalendarService : ICalendarService
    {
        public Task<IEnumerable<string>> GetEventsForDateAsync(DateOnly date)
        {
            IEnumerable<string> events = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                ? []
                : ["09:00 Daily Standup (30m)", "14:00 Architecture Review (1h)"];
            return Task.FromResult(events);
        }
    }
}
