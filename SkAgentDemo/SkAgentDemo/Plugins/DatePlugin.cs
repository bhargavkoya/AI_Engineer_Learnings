using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkAgentDemo.Plugins
{
    public class DatePlugin
    {
        [KernelFunction("get_current_date")]
        [Description("Returns today's date in ISO 8601 format (YYYY-MM-DD). Use when the user asks what day or date it is.")]
        public string GetCurrentDate() =>
            // DateTimeOffset.UtcNow ensures the result is timezone-safe
            DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        [KernelFunction("get_day_of_week")]
        [Description("Returns the current day of the week (e.g., Monday). Use when the user asks what day of the week it is.")]
        public string GetDayOfWeek() =>
            DateTimeOffset.UtcNow.DayOfWeek.ToString();

        [KernelFunction("get_current_time")]
        [Description("Returns the current UTC time in HH:mm format. Use when the user asks what time it is.")]
        public string GetCurrentTime() =>
            DateTimeOffset.UtcNow.ToString("HH:mm") + " UTC";
    }
}
