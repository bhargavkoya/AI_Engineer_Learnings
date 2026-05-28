using AIAssistant.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIAssistant.Plugins
{
    public class CalendarPlugin
    {
        private readonly ICalendarService _calendar;

        public CalendarPlugin(ICalendarService calendar) => _calendar = calendar;

        [KernelFunction("get_events")]
        [Description(
            "Get all calendar events for a specific date. " +
            "Returns event titles, start times, and durations. " +
            "Use when the user asks about their schedule, meetings, or what is on their calendar.")]
        public async Task<string> GetEventsAsync(
            [Description("Date to query in yyyy-MM-dd format")] string date)
        {
            var parsed = DateOnly.Parse(date);
            var events = await _calendar.GetEventsForDateAsync(parsed);
            return events.Any()
                ? string.Join(", ", events)
                : $"No events found for {date}.";
        }
    }
}
