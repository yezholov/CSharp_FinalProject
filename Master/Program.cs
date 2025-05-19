using Master.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("[Master] Process started. Waiting for agents to connect...");

        var aggregator = new DataAggregator();
        var resultPrinter = new ResultPrinter();

        var agent1Task = ProcessAgentAsync("agent1_pipe", "Agent 1", aggregator);
        var agent2Task = ProcessAgentAsync("agent2_pipe", "Agent 2", aggregator);

        await Task.WhenAll(agent1Task, agent2Task);

        Console.WriteLine("\n[Master] All agents have finished processing their directories.");

        // Display final results
        resultPrinter.PrintToConsole(aggregator.GetResults());

        Console.WriteLine("[Master] Process finished.");
    }

    static async Task ProcessAgentAsync(
        string agentPipeName,
        string agentName,
        DataAggregator aggregator
    )
    {
        using var pipeServer = new PipeServer(agentPipeName, agentName);
        try
        {
            await pipeServer.WaitForConnectionAsync();
            await pipeServer.ReceiveDataAsync(aggregator);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{agentName}] Error processing: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[{agentName}] Finished processing");
        }
    }
}
