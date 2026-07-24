namespace TicketML.Api.Models
{
    // Must match the training-time TicketRecord's feature columns exactly -
    // same names, same types - or the pipeline's transforms will fail to bind.
    public class TicketRecordInput
    {
        public float HoursSinceCreated { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "Billing";
        public string CustomerTier { get; set; } = "Free";
        public float NumReopens { get; set; }
        public float AssignedTeamSize { get; set; }
    }

    public class TicketPredictionOutput
    {
        [Microsoft.ML.Data.ColumnName("PredictedLabel")]
        public bool WillBreachSla { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
