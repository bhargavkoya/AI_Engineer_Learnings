using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketML.Training
{
    // Shared record used for both classification and regression demos -
    // same underlying ticket, two different labels (BreachedSla, ResolutionHours).
    public class TicketRecord
    {
        public float HoursSinceCreated { get; set; }
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Category { get; set; } = "Billing"; // Billing, Auth, Performance, Integration
        public string CustomerTier { get; set; } = "Free"; // Free, Pro, Enterprise
        public float NumReopens { get; set; }
        public float AssignedTeamSize { get; set; }
        public bool BreachedSla { get; set; }
        public float Weight { get; set; } = 1f; // Class-imbalance weight for the classifier
        public float ResolutionHours { get; set; }
    }

    public class TicketPrediction
    {
        [Microsoft.ML.Data.ColumnName("PredictedLabel")]
        public bool WillBreachSla { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }

    public class WeeklyVolume
    {
        public float TicketCount { get; set; }
    }

    public class TicketVolumeForecast
    {
        public float[] ForecastedTicketVolume { get; set; } = System.Array.Empty<float>();
        public float[] LowerBound { get; set; } = System.Array.Empty<float>();
        public float[] UpperBound { get; set; } = System.Array.Empty<float>();
    }
}
