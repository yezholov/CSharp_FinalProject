using Master.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up loggers
        var masterLogger = new Logger(0);
        var agentLoggers = new Logger[] { new Logger(1), new Logger(2) };
        var masterAgentLoggers = new Logger[] { new Logger(11), new Logger(12) };

        // Set CPU Affinity if supported
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) // Check if the OS is not macOS
        {
            var coreCount = Environment.ProcessorCount; // Get the number of cores
            try
            {
                var process = Process.GetCurrentProcess(); // Get the current process
                process.ProcessorAffinity = new IntPtr(1); // Set the process to run on core 0
                masterLogger.Log(
                    $"Process assigned to CPU core 0 (of {coreCount} cores available)"
                );
            }
            catch (Exception ex)
            {
                masterLogger.Warning(
                    $"Warning: Could not set processor affinity: {ex.Message}"
                );
            }
        }
        else
        {
            masterLogger.Warning("CPU affinity is not supported on macOS"); //Warning if CPU affinity is not supported on macOS
        }

        // Parallel launch of agents (launching applications)
        var agentProcesses = new List<Process>();
        var launchTasks = new[]
        {
            Task.Run(() => LaunchAgent(1, "../TestDataA", agentProcesses, [masterLogger, agentLoggers[0]])),
            Task.Run(() => LaunchAgent(2, "../TestDataB", agentProcesses, [masterLogger, agentLoggers[1]]))
        };

        await Task.WhenAll(launchTasks); // Start all the agents

        var aggregator = new DataAggregator(); // Aggregate the results

        // Connect agents and get data in parallel
        masterLogger.Log("Waiting for agents to connect...");
        var agentTasks = new List<Task>();
        for (int i = 1; i <= 2; i++)
        {
            agentTasks.Add(ProcessAgentAsync(
                $"agent{i}_pipe", // Pipe name
                $"Agent {i}", // Agent name
                aggregator, // Data aggregator
                masterAgentLoggers[i - 1] // Logger
            ));
        }
        await Task.WhenAll(agentTasks); // Connect and get data in parallel

        masterLogger.Spacer();
        masterLogger.Log("All agents have finished processing their directories.");

        // Display final results
        var resultPrinter = new ResultPrinter();
        resultPrinter.PrintToConsole(aggregator.GetResults());

        masterLogger.Log("Process finished.");

        // Cleanup agent processes (Kill processes)
        foreach (var process in agentProcesses)
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
            process.Dispose();
        }
    }

    static void LaunchAgent(int agentId, string directory, List<Process> processes, Logger[] loggers)
    {
        Logger masterLogger = loggers[0];
        Logger agentLogger = loggers[1];

        try
        {
            // Setup process
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet", // Dotnet executable
                Arguments = $"run --project ../Agent {directory} {agentId}", // Arguments
                UseShellExecute = false, // Don't use shell
                RedirectStandardOutput = true, // Redirect output
                RedirectStandardError = true, // Redirect error
            };
            //Create process
            var process = new Process { StartInfo = startInfo };

            // Log output
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    agentLogger.Log(e.Data);
            };
            // Log error
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    agentLogger.Error(e.Data);
            };

            // Start process and log output and error
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Add process to list (lock to avoid race condition)
            lock (processes)
            {
                processes.Add(process);
            }
        }
        catch (Exception ex)
        {
            masterLogger.Error($"Failed to start Agent {agentId}: {ex.Message}");
        }
    }

    static async Task ProcessAgentAsync(
        string agentPipeName,
        string agentName,
        DataAggregator aggregator,
        Logger logger
    )
    {
        // Create pipe server
        using var pipeServer = new PipeServer(agentPipeName, agentName, logger);
        try
        {
            await pipeServer.WaitForConnectionAsync(); // Wait for connection Agent
            await pipeServer.ReceiveDataAsync(aggregator); // Receive data from Agent
        }
        catch (Exception ex)
        {
            logger.Error($"Error processing: {ex.Message}");
        }
        finally
        {
            logger.Log($"Finished processing");
        }
    }
}
