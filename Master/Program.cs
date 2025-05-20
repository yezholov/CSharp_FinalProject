using Master.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static async Task Main(string[] args)
    {
        // Set CPU Affinity if supported
        var coreCount = Environment.ProcessorCount;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var process = Process.GetCurrentProcess();
                process.ProcessorAffinity = new IntPtr(1); // Core 0
                Console.WriteLine(
                    $"Master process assigned to CPU core 0 (of {coreCount} cores available)"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set processor affinity: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"CPU affinity is not supported on macOS");
        }

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
