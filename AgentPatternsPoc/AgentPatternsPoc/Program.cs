using AgentPatternsPoc.Demos;

const string BugReport = """
    Bug: NullReferenceException in OrderService.ProcessAsync (line 142).
    Occurs when processing orders for users with no saved payment method.
    Affects the Checkout service. Reported by 3 Enterprise customers since 09:00 UTC.
    """;

await SingleAgentDemo.RunAsync(BugReport,new CancellationToken());
await OrchestratorWorkerDemo.RunAsync(BugReport);
await GroupChatDemo.RunAsync(BugReport);