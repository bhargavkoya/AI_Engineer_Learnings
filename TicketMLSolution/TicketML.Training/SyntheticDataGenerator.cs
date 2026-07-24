using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketML.Training
{
    public static class SyntheticDataGenerator
    {
        private static readonly string[] Priorities = { "Low", "Medium", "High", "Critical" };
        private static readonly string[] Categories = { "Billing", "Auth", "Performance", "Integration" };
        private static readonly string[] Tiers = { "Free", "Pro", "Enterprise" };

        // Generates ticket rows with a deliberate ~15% SLA breach rate and a
        // right-skewed resolution-time distribution, mirroring real helpdesk data.
        public static List<TicketRecord> GenerateTickets(int count, int seed = 42)
        {
            var rng = new Random(seed);
            var tickets = new List<TicketRecord>(count);

            for (int i = 0; i < count; i++)
            {
                var priority = Priorities[rng.Next(Priorities.Length)];
                var category = Categories[rng.Next(Categories.Length)];
                var tier = Tiers[rng.Next(Tiers.Length)];
                var reopens = rng.Next(0, 4);
                var teamSize = rng.Next(1, 6);
                var hoursSinceCreated = (float)(rng.NextDouble() * 72);

                // Higher priority + more reopens + smaller team = more likely to breach.
                var riskScore = (priority == "Critical" ? 3 : priority == "High" ? 2 : 0)
                    + reopens * 1.5 - teamSize * 0.4 + rng.NextDouble() * 2;
                var breached = riskScore > 4.0;

                // Resolution hours: right-skewed - most tickets close fast, a tail drags long.
                var baseHours = 2 + reopens * 6 + (priority == "Critical" ? 1 : priority == "High" ? 4 : 10);
                var resolutionHours = (float)(baseHours * (1 + rng.NextDouble() * rng.NextDouble() * 3));

                tickets.Add(new TicketRecord
                {
                    HoursSinceCreated = hoursSinceCreated,
                    Priority = priority,
                    Category = category,
                    CustomerTier = tier,
                    NumReopens = reopens,
                    AssignedTeamSize = teamSize,
                    BreachedSla = breached,
                    Weight = breached ? 5f : 1f, // Upweight the minority class for SDCA
                    ResolutionHours = resolutionHours
                });
            }

            return tickets;
        }

        // Generates 104 weeks (2 years) of ticket volume with upward trend +
        // quarterly seasonality + noise, for the forecasting demo.
        public static List<WeeklyVolume> GenerateWeeklyVolume(int weeks = 104, int seed = 7)
        {
            var rng = new Random(seed);
            var series = new List<WeeklyVolume>(weeks);

            for (int week = 0; week < weeks; week++)
            {
                var trend = 80 + week * 0.35;
                var seasonal = 15 * Math.Sin(2 * Math.PI * week / 13.0); // ~quarterly cycle
                var noise = rng.NextDouble() * 10 - 5;
                series.Add(new WeeklyVolume { TicketCount = (float)(trend + seasonal + noise) });
            }

            return series;
        }
    }
}
