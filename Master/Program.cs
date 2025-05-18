using Master.Services;
using Master.Models;

class Program
{
    // List of files 
    private static readonly List<string> filesToProcess =
        ["test_data.txt", "test_data2.txt"];

    static async Task Main(string[] args)
    {
        Console.WriteLine("[Master] Process started. Preparing to connect to agents...");

        List<string> agentPipeNames = ["agent1_pipe", "agent2_pipe"];

        var aggregator = new DataAggregator();
        var resultPrinter = new ResultPrinter();
        var agentTasks = new List<Task>();


        for (int i = 0; i < agentPipeNames.Count; i++)
        {
            if (i < filesToProcess.Count)
            {
                string pipeName = agentPipeNames[i];
                string fileForThisAgent = filesToProcess[i];
                agentTasks.Add(ProcessAgentAsync(pipeName, fileForThisAgent, aggregator));
            }
            else
            {
                Console.WriteLine(
                    $"[Master] No file assigned to agent on pipe '{agentPipeNames[i]}' (not enough files)."
                );
            }
        }

        await Task.WhenAll(agentTasks);


        // Display final results
        resultPrinter.PrintToConsole(aggregator.GetResults());

        Console.WriteLine("[Master] Process finished.");
    }


    static async Task ProcessAgentAsync(
        string agentPipeName,
        string fileNameToSend,
        DataAggregator aggregator
    )
    {
        using (var pipeServer = new PipeServer(agentPipeName))
        {
            try
            {
                await pipeServer.WaitForConnectionAsync();

                if (pipeServer.IsClientConnected)
                {
                    await pipeServer.SendCommandAsync(fileNameToSend);

                    List<string> rawDataLines = await pipeServer.ReceiveDataAsync();

                    if (rawDataLines.Any())
                    {
                        foreach (var line in rawDataLines)
                        {
                            AgentData? agentData = AgentData.Parse(line);
                            if (agentData != null)
                            {
                                aggregator.Aggregate(agentData);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(
                            $"[Master] No data lines received from agent {agentPipeName} for file {fileNameToSend}."
                        );
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"[Master] No agent connected to pipe '{agentPipeName}' during the wait period."
                    );
                }
            }
            catch (TimeoutException tex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    $"[Master] Timeout connecting or communicating with agent on {agentPipeName}: {tex.Message}"
                );
                Console.ResetColor();
            }
            catch (IOException ioex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    $"[Master] IO Exception with agent on {agentPipeName} (for file {fileNameToSend}): {ioex.Message}. Agent might have disconnected."
                );
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"[Master] Error processing agent on {agentPipeName} (for file {fileNameToSend}): {ex.Message}"
                );
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine(
                    $"[Master] Finished processing for agent on pipe: {agentPipeName} (file: {fileNameToSend})"
                );
            }
        }
    }
}
