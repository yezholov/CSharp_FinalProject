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
        // Check if the arguments are valid
        if (args.Length < 2)
        {
            Console.WriteLine("Command: Agent <directory_path> <agent_id_for_pipe>"); // Write the right arguments
            return;
        }
        // Normalize and check if the directory path is valid
        string directoryPath = FileHelper.NormalizePath(args[0]);
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Error: Directory '{directoryPath}' does not exist.");
            return;
        }

        // Check if the agent ID is a positive integer
        if (!int.TryParse(args[1], out int agentId) || agentId <= 0)
        {
            Console.WriteLine("Error: Agent ID must be a positive integer for pipe naming.");
            return;
        }
        // Set the pipe name and agent name
        string pipeName = $"agent{agentId}_pipe";
        string agentName = $"Agent {agentId}";

        // Set CPU Affinity if supported
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) // Check if the OS is not macOS
        {
            var coreCount = Environment.ProcessorCount; // Get the number of cores
            var targetCore = agentId % coreCount; // Set the target core (id_Agent % coreCount)
            try
            {
                var process = Process.GetCurrentProcess(); // Get the current process
                process.ProcessorAffinity = new IntPtr(1 << targetCore); // Set the process to run on the target core
                Console.WriteLine(
                    $"Assigned to CPU core {targetCore} (of {coreCount} cores available)"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set processor affinity: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Warning: CPU affinity is not supported on macOS");
        }

        // Create a blocking collection to store the results (for communication between processing and sending data)
        var resultQueue = new BlockingCollection<FileIndex>();

        // Create a pipe client to send data to the master
        using var pipeClient = new PipeClient(pipeName, agentName);

        // Create a file scanner to scan the files in the directory
        var scanner = new FileScanner();

        try
        {
            if (!pipeClient.IsConnected) // Check if the pipe client is connected
            {
                await pipeClient.ConnectAsync(); // Connect to the pipe client
            }

            var filesInDirectory = await scanner.GetTextFilesAsync(directoryPath); // Get the text files in the directory

            if (!filesInDirectory.Any()) // Check if there are any text files in the directory
            {
                Console.WriteLine($"No text files found in directory.");
                return;
            }

            Console.WriteLine($"Found {filesInDirectory.Count} text files to process.");

            // Task for processing files sequentially
            var processingTask = Task.Run(async () =>
            {
                try
                {
                    foreach (var file in filesInDirectory) // For each file
                    {
                        Console.WriteLine($"Processing file: {Path.GetFileName(file)}");
                        var fileIndexResult = await scanner.ScanFileAsync(file); // Indexing the file
                        resultQueue.Add(fileIndexResult); // Add the result to the collection (for sending data)
                    }
                }
                finally
                {
                    Console.WriteLine($"Processing completed.");
                    resultQueue.CompleteAdding(); // Set complete flag to collection
                }
            });

            // Task for sending results
            var sendingTask = Task.Run(async () =>
            {
                try
                {
                    foreach (var result in resultQueue.GetConsumingEnumerable()) // For each result (while is not complete)
                    {
                        await pipeClient.SendDataAsync(result); // Send the result to the master
                        Console.WriteLine($"Sent result to Master.");
                    }
                    await pipeClient.SendEndOfDataMarker(); // Send the end of data marker (flag for Master)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: Can't send data: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"Sending completed.");
                }
            });

            await Task.WhenAll(processingTask, sendingTask); // Start both tasks
            Console.WriteLine($"All tasks completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Unexpected error - '{ex.Message}'.");
            return;
        }
    }
}
