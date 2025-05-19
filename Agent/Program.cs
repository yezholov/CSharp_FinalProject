using Agent.Helpers;
using Agent.Services;

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
            Console.WriteLine($"Error: Directory '{directoryPath}' does not exist.");
            return;
        }

        if (!int.TryParse(args[1], out int agentId) || agentId <= 0)
        {
            Console.WriteLine("Error: Agent ID must be a positive integer for pipe naming.");
            return;
        }

        string pipeName = $"agent{agentId}_pipe";
        string agentName = $"Agent {agentId}";

        using var pipeClient = new PipeClient(pipeName, agentName);
        var scanner = new FileScanner();

        Console.WriteLine(
            $"Agent {agentId} started. \nDirectory: '{directoryPath}'. \nPipe: '{pipeName}'. \nWaiting for Master...\n"
        );

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
            }
            else
            {
                Console.WriteLine(
                    $"[{agentName}] Found {filesInDirectory.Count} text files to process."
                );
                foreach (var file in filesInDirectory)
                {
                    Console.WriteLine($"[{agentName}] Processing file: {Path.GetFileName(file)}");
                    var fileIndexResult = await scanner.ScanFileAsync(file);
                    await pipeClient.SendDataAsync(fileIndexResult);
                }
            }

            await pipeClient.SendEndOfDataMarker();
            Console.WriteLine($"[{agentName}] Processing completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Agent {agentId}: Unexpected error - '{ex.Message}'.");
            return;
        }

        pipeClient.Dispose();
    }
}
