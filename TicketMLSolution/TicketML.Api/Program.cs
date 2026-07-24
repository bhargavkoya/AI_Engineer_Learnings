using Microsoft.Extensions.ML;
using TicketML.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// PredictionEnginePool, not a singleton PredictionEngine - this is the line
// that would have prevented the opening incident.
builder.Services.AddPredictionEnginePool<TicketRecordInput, TicketPredictionOutput>()
    .FromFile(modelName: "TicketTriage", filePath: "MLModels/ticket-triage-model.zip", watchForChanges: true);

var app = builder.Build();
app.MapControllers();
app.Run();