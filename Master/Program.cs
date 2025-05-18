using Master.Services;
using Master.Models;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Master process started.");
        const string agentPipeName = "agent1_pipe";
        const string fileToSendToAgent = "test_data.txt";

        var aggregator = new DataAggregator();
        var resultPrinter = new ResultPrinter();

        using (var pipeServer = new PipeServer(agentPipeName))
        {
            try
            {
                await pipeServer.WaitForConnectionAsync();

                if (pipeServer.IsClientConnected)
                {
                    await pipeServer.SendCommandAsync(fileToSendToAgent);
                    List<string> rawDataLines = await pipeServer.ReceiveDataAsync();

                    if (rawDataLines.Any())
                    {
                        foreach (var line in rawDataLines)
                        {
                            AgentData agentData = AgentData.Parse(line);
                            aggregator.Aggregate(agentData);
                        }

                        resultPrinter.PrintToConsole(aggregator.GetResults());
                    }
                    else
                    {
                        Console.WriteLine("No data lines received from agent.");
                        resultPrinter.PrintToConsole(aggregator.GetResults());
                    }
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
        }
        Console.WriteLine("Master process finished.");
    }
}
