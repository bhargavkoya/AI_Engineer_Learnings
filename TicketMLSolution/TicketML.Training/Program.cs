using Microsoft.ML;
using TicketML.Training;

var mlContext = new MLContext(seed: 42);

var tickets = SyntheticDataGenerator.GenerateTickets(count: 5000);
var weeklyVolume = SyntheticDataGenerator.GenerateWeeklyVolume(weeks: 104);

ClassificationDemo.Run(mlContext, tickets);
Console.WriteLine();
RegressionDemo.Run(mlContext, tickets);
Console.WriteLine();
ForecastingDemo.Run(mlContext, weeklyVolume);