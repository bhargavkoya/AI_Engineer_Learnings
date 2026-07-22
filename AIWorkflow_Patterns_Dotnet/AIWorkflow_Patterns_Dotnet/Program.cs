using AIWorkflow_Patterns_Dotnet;
using AIWorkflow_Patterns_Dotnet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<RefundApprovalDbContext>(options =>
    options.UseSqlite("Data Source=refund_approvals.db"));

builder.Services.AddChatClient(
    new OpenAIClient(builder.Configuration["OpenAI:ApiKey"]!)
        .GetChatClient("gpt-4o-mini")
        .AsIChatClient());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/tickets/process", async (
    TicketInput input,
    TicketAnalysisWorkflow analysisWorkflow,
    AssessRefundStep assessStep,
    RefundApprovalDbContext db,
    CancellationToken ct) =>
{
    var analysis = await analysisWorkflow.AnalyzeAsync(input.TicketText, ct);       // Fan-out
    var assessment = await assessStep.ExecuteAsync(input, analysis, ct);           // Chain

    if (!assessment.RecommendRefund)
    {
        return Results.Ok(new { assessment, approvalRequired = false });
    }

    var approvalRequest = new RefundApprovalRequest
    {
        CustomerId = input.CustomerId,
        Amount = assessment.Amount,
        AiJustification = assessment.Reason
    };

    db.RefundApprovals.Add(approvalRequest);
    await db.SaveChangesAsync(ct); // Pause point - HITL takes over from here.

    return Results.Accepted(value: new { approvalRequest.Id, approvalRequired = true });
});

app.Run();