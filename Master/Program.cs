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
                    $"[Master] Process assigned to CPU core 0 (of {coreCount} cores available)"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Master] Warning: Could not set processor affinity: {ex.Message}"
                );
            }
        }
        else
        {
            Console.WriteLine($"[Master] CPU affinity is not supported on macOS");
        }

        Console.WriteLine("[Master] Process started. Starting agents...");

        // Parallel launch of agents
        var agentProcesses = new List<Process>();
        var launchTasks = new[]
        {
            Task.Run(() => LaunchAgent(1, "../TestDataA", agentProcesses)),
            Task.Run(() => LaunchAgent(2, "../TestDataB", agentProcesses))
        };

        await Task.WhenAll(launchTasks);

        Console.WriteLine("[Master] Waiting for agents to connect...");

        var aggregator = new DataAggregator();
        var resultPrinter = new ResultPrinter();

        var agentTasks = new List<Task>();
        for (int i = 1; i <= 2; i++)
        {
            var agentTask = ProcessAgentAsync($"agent{i}_pipe", $"Agent {i}", aggregator);
            agentTasks.Add(agentTask);
        }

        await Task.WhenAll(agentTasks);

        Console.WriteLine("\n[Master] All agents have finished processing their directories.");

        // Display final results
        resultPrinter.PrintToConsole(aggregator.GetResults());

        Console.WriteLine("[Master] Process finished.");

        // Cleanup agent processes
        foreach (var process in agentProcesses)
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
            process.Dispose();
        }
    }

    static void LaunchAgent(int agentId, string directory, List<Process> processes)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project ../Agent {directory} {agentId}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            var process = new Process { StartInfo = startInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"{e.Data}");
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"{e.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            lock (processes)
            {
                processes.Add(process);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Master] Failed to start Agent {agentId}: {ex.Message}");
        }
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
            Console.WriteLine($"[Master / {agentName}] Error processing: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[Master / {agentName}] Finished processing");
        }
    }
}
