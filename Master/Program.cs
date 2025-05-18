using Master.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Master process started.");
        const string agentPipeName = "agent1_pipe"; // test name agent1_pipe
        const string fileToSendToAgent = "test_data.txt"; // test file test_data.txt

        using (var pipeServer = new PipeServer(agentPipeName))
        {
            try
            {
                await pipeServer.WaitForConnectionAsync();

                if (pipeServer.IsClientConnected)
                {
                    await pipeServer.SendCommandAsync(fileToSendToAgent);

                    List<string> indexedData = await pipeServer.ReceiveDataAsync();

                    Console.WriteLine("\n--- Received Indexed Data from Agent ---");
                    if (indexedData.Any())
                    {
                        foreach (var line in indexedData)
                        {
                            Console.WriteLine(line);
                        }
                    }
                    else
                    {
                        Console.WriteLine(
                            "No data received from agent or agent sent no indexed lines."
                        );
                    }
                    Console.WriteLine("--- End of Received Data ---");
                }
                else
                {
                    Console.WriteLine(
                        $"No agent connected to pipe '{agentPipeName}' during the wait period."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred in the Master process: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                if (pipeServer.IsClientConnected)
                {
                    pipeServer.CloseClientConnection();
                }
            }
        }
        Console.WriteLine("Master process finished.");
    }
}
