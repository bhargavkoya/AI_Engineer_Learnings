using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketML.Training
{
    public static class ClassificationDemo
    {
        public static void Run(MLContext mlContext, List<TicketRecord> tickets)
        {
            var split = mlContext.Data.TrainTestSplit(
                mlContext.Data.LoadFromEnumerable(tickets), testFraction: 0.2, seed: 42);

            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[]
                {
                new InputOutputColumnPair("PriorityEncoded", "Priority"),
                new InputOutputColumnPair("CategoryEncoded", "Category"),
                new InputOutputColumnPair("CustomerTierEncoded", "CustomerTier"),
            })
                .Append(mlContext.Transforms.Concatenate("Features",
                    "PriorityEncoded", "CategoryEncoded", "CustomerTierEncoded",
                    "NumReopens", "AssignedTeamSize", "HoursSinceCreated"))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                // Weight column is the non-trivial decision here: without it, the
                // model converges toward always predicting "no breach."
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "BreachedSla",
                    featureColumnName: "Features",
                    exampleWeightColumnName: "Weight"));

            var model = pipeline.Fit(split.TrainSet);
            var predictions = model.Transform(split.TestSet);

            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "BreachedSla");
            Console.WriteLine("--- Classification: SLA breach prediction ---");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC-PR:   {metrics.AreaUnderPrecisionRecallCurve:F3} (trust this over accuracy)");
            Console.WriteLine($"F1 Score: {metrics.F1Score:F3}");

            mlContext.Model.Save(model, split.TrainSet.Schema, "ticket-triage-model.zip");
            Console.WriteLine("Saved ticket-triage-model.zip");
        }
    }
}
