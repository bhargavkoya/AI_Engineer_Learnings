namespace TokenVisualizer.Api.Models
{
    public class CostEstimate
    {
        public string ModelName { get; set; } = "";
        public double InputCostUsd { get; set; }
        public double OutputCostUsd { get; set; }
        public double TotalCostUsd { get; set; }
        public string Note { get; set; } = "";
    }
}
