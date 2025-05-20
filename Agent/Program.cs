using Agent.Helpers;
using Agent.Services;
using Agent.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Command: Agent <directory_path> <agent_id_for_pipe>");
            return;
        }

        string directoryPath = FileHelper.NormalizePath(args[0]);
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"[Agent] Error: Directory '{directoryPath}' does not exist.");
            return;
        }

        if (!int.TryParse(args[1], out int agentId) || agentId <= 0)
        {
            Console.WriteLine(
                "[Agent] Error: Agent ID must be a positive integer for pipe naming."
            );
            return;
        }
        string pipeName = $"agent{agentId}_pipe";
        string agentName = $"Agent {agentId}";
        // Set CPU Affinity if supported
        var coreCount = Environment.ProcessorCount;
        var targetCore = agentId % coreCount;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var process = Process.GetCurrentProcess();
                process.ProcessorAffinity = new IntPtr(1 << targetCore);
                Console.WriteLine(
                    $"[{agentName}] Assigned to CPU core {targetCore} (of {coreCount} cores available)"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[{agentName}] Warning: Could not set processor affinity: {ex.Message}"
                );
            }
        }
        else
        {
            Console.WriteLine($"[{agentName}] CPU affinity is not supported on macOS");
        }

        var resultQueue = new BlockingCollection<FileIndex>();

        using var pipeClient = new PipeClient(pipeName, agentName);
        var scanner = new FileScanner();

        //Console.WriteLine(
        //    $"[Agent {agentId}] Started. \nDirectory: '{directoryPath}'. \nPipe: '{pipeName}'. \nWaiting for Master...\n"
        //);

        try
        {
            if (!pipeClient.IsConnected)
            {
                await pipeClient.ConnectAsync();
            }

            var filesInDirectory = await scanner.GetTextFilesAsync(directoryPath);
            if (!filesInDirectory.Any())
            {
                Console.WriteLine($"[{agentName}] No text files found in directory.");
                return;
            }

            Console.WriteLine(
                $"[{agentName}] Found {filesInDirectory.Count} text files to process."
            );

            // Task for processing files sequentially
            var processingTask = Task.Run(async () =>
            {
                try
                {
                    foreach (var file in filesInDirectory)
                    {
                        Console.WriteLine(
                            $"[{agentName}] Processing file: {Path.GetFileName(file)}"
                        );
                        var fileIndexResult = await scanner.ScanFileAsync(file);
                        resultQueue.Add(fileIndexResult);
                    }
                }
                finally
                {
                    Console.WriteLine($"[{agentName}] Processing completed.");
                    resultQueue.CompleteAdding();
                }
            });

            // Task for sending results
            var sendingTask = Task.Run(async () =>
            {
                try
                {
                    foreach (var result in resultQueue.GetConsumingEnumerable())
                    {
                        await pipeClient.SendDataAsync(result);
                        Console.WriteLine($"[{agentName}] Sent result to Master.");
                    }
                    await pipeClient.SendEndOfDataMarker();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{agentName}] Error sending data: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"[{agentName}] Sending completed.");
                }
            });

            await Task.WhenAll(processingTask, sendingTask);
            Console.WriteLine($"[{agentName}] All tasks completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Agent {agentId}: Unexpected error - '{ex.Message}'.");
            return;
        }
    }
}
