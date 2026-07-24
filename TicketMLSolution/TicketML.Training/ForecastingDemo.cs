using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketML.Training
{
    public static class ForecastingDemo
    {
        public static void Run(MLContext mlContext, List<WeeklyVolume> weeklyVolume)
        {
            var dataView = mlContext.Data.LoadFromEnumerable(weeklyVolume);

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedTicketVolume",
                inputColumnName: "TicketCount",
                windowSize: 12,   // ~quarterly cycle - chosen for only 2 years of history
                seriesLength: 52, // one year feeds the seasonal decomposition
                trainSize: weeklyVolume.Count,
                horizon: 6,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBound",
                confidenceUpperBoundColumn: "UpperBound");

            var forecastingTransformer = forecastingPipeline.Fit(dataView);

            // Keep the time-series engine (not just the ITransformer) so we can
            // checkpoint state and update with new weeks later without retraining.
            var forecastEngine = forecastingTransformer.CreateTimeSeriesEngine<WeeklyVolume, TicketVolumeForecast>(mlContext);
            var forecast = forecastEngine.Predict();

            Console.WriteLine("--- Forecasting: next 6 weeks of ticket volume ---");
            for (int i = 0; i < forecast.ForecastedTicketVolume.Length; i++)
            {
                Console.WriteLine(
                    $"Week +{i + 1}: {forecast.ForecastedTicketVolume[i]:F1} " +
                    $"(95% CI: {forecast.LowerBound[i]:F1} - {forecast.UpperBound[i]:F1})");
            }

            forecastEngine.CheckPoint(mlContext, "ticket-volume-forecast-state.zip");
            Console.WriteLine("Checkpointed forecast state to ticket-volume-forecast-state.zip");
        }
    }
}
