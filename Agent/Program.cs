using Agent.Helpers;
using Agent.Services;

class Program
{
    private const string ShutdownCommand = "SHUTDOWN";

    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Command: Agent <base_directory_path> <agent_id_for_pipe>");
            return;
        }

        string baseDirectoryPath = FileHelper.NormalizePath(args[0]);
        if (!int.TryParse(args[1], out int agentId) || agentId <= 0)
        {
            Console.WriteLine("Error: Agent ID must be a positive integer for pipe naming.");
            return;
        }

        string pipeName = $"agent{agentId}_pipe";
        using var pipeClient = new PipeClient(pipeName);
        var scanner = new FileScanner();
        bool keepRunning = true;

        Console.WriteLine(
            $"Agent {agentId} started. \nDirectory: '{baseDirectoryPath}'. \nPipe: '{pipeName}'. \nWaiting for Master..."
        );

        while (keepRunning)
        {
            try
            {
                if (!pipeClient.IsConnected)
                {
                    await pipeClient.ConnectAsync();
                }

                string? command = await pipeClient.ReceiveCommandAsync();

                if (string.IsNullOrEmpty(command))
                {
                    Console.WriteLine($"Agent {agentId}: Connection lost or empty command.");
                    pipeClient.Close();
                    await Task.Delay(2000);
                    continue;
                }

                if (command.Equals(ShutdownCommand, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Agent {agentId}: Shutdown command received.");
                    keepRunning = false;
                    continue;
                }

                string fileNameToProcess = command;
                string fullPathToFile = Path.Combine(baseDirectoryPath, fileNameToProcess);
                var fileIndexResult = await scanner.ScanFileAsync(fullPathToFile);
                await pipeClient.SendDataAsync(fileIndexResult);
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Agent {agentId}: Connection to Master timed out.");
                pipeClient.Close();
                await Task.Delay(5000);
            }
            catch (FileNotFoundException fnfEx)
            {
                Console.WriteLine(
                    $"Agent {agentId}: File '{fnfEx.FileName}' not found. Waiting for next command."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Agent {agentId}: Unexpected error - '{ex.Message}'. Attempting to recover pipe connection..."
                );
                pipeClient.Close();
                await Task.Delay(5000);
            }
        }

        pipeClient.Close();
        Console.WriteLine($"Agent {agentId} has shut down.");
    }
}
