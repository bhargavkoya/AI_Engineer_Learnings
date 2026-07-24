using Microsoft.ML;

namespace TicketML.Training
{
    public class LogLabelOutput
    {
        public float LogResolutionHours { get; set; }
    }

    public static class RegressionDemo
    {
        public static void Run(MLContext mlContext, List<TicketRecord> tickets)
        {
            var split = mlContext.Data.TrainTestSplit(
                mlContext.Data.LoadFromEnumerable(tickets), testFraction: 0.2, seed: 42);

            var featurePipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[]
                {
                new InputOutputColumnPair("PriorityEncoded", "Priority"),
                new InputOutputColumnPair("CategoryEncoded", "Category"),
                new InputOutputColumnPair("CustomerTierEncoded", "CustomerTier"),
            })
                .Append(mlContext.Transforms.Concatenate("Features",
                    "PriorityEncoded", "CategoryEncoded", "CustomerTierEncoded",
                    "NumReopens", "AssignedTeamSize", "HoursSinceCreated"))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                // Log-transform the skewed target - the non-trivial decision for this demo.
                .Append(mlContext.Transforms.CustomMapping<TicketRecord, LogLabelOutput>(
                    (input, output) => output.LogResolutionHours = MathF.Log(1f + input.ResolutionHours),
                    contractName: null));

            var sdcaModel = featurePipeline
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "LogResolutionHours"))
                .Fit(split.TrainSet);

            var fastTreeModel = featurePipeline
                .Append(mlContext.Regression.Trainers.FastTree(labelColumnName: "LogResolutionHours"))
                .Fit(split.TrainSet);

            var sdcaMetrics = mlContext.Regression.Evaluate(
                sdcaModel.Transform(split.TestSet), labelColumnName: "LogResolutionHours");
            var fastTreeMetrics = mlContext.Regression.Evaluate(
                fastTreeModel.Transform(split.TestSet), labelColumnName: "LogResolutionHours");

            Console.WriteLine("--- Regression: resolution time prediction ---");
            Console.WriteLine($"Sdca      RSquared: {sdcaMetrics.RSquared:F3}");
            Console.WriteLine($"FastTree  RSquared: {fastTreeMetrics.RSquared:F3}");

            var winner = fastTreeMetrics.RSquared > sdcaMetrics.RSquared ? "FastTree" : "Sdca";
            Console.WriteLine($"Selected trainer: {winner}");
        }
    }
}
