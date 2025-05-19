using System.IO.Pipes;
using Master.Models;

namespace Master.Services
{
    public class PipeServer : IDisposable
    {
        private NamedPipeServerStream? _pipeServer;
        private StreamReader? _streamReader;
        private readonly string _pipeName;
        private readonly string _agentName;

        public bool IsClientConnected => _pipeServer?.IsConnected ?? false;

        public PipeServer(string pipeName, string agentName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                throw new ArgumentNullException(nameof(pipeName));
            _pipeName = pipeName;
            _agentName = agentName;
        }

        public async Task WaitForConnectionAsync(int timeout = 10000)
        {
            try
            {
                _pipeServer?.Dispose();
                _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In);

                Console.WriteLine(
                    $"[Master] Pipe server '{_pipeName}' created. Waiting for agent connection..."
                );
                await _pipeServer.WaitForConnectionAsync();

                if (_pipeServer.IsConnected)
                {
                    _streamReader = new StreamReader(_pipeServer);
                    Console.WriteLine($"[{_agentName}] Connected.");
                }
                else
                {
                    Console.WriteLine($"[{_agentName}] Connection failed after waiting.");
                    _pipeServer?.Dispose();
                    _pipeServer = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_agentName}] Error connecting: {ex.Message}");
                _pipeServer?.Dispose();
                _pipeServer = null;
                throw;
            }
        }

        public async Task ReceiveDataAsync(DataAggregator aggregator)
        {
            if (!IsClientConnected || _streamReader == null)
            {
                throw new InvalidOperationException($"[Master] {_agentName} is not connected.");
            }

            try
            {
                Console.WriteLine($"[{_agentName}] Sending data...");
                string? line;
                int processedLines = 0;

                while ((line = await _streamReader.ReadLineAsync()) != null)
                {
                    if (line.Equals("END_OF_DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(
                            $"[{_agentName}] Finished sending data. Processed {processedLines} entries."
                        );
                        return;
                    }

                    try
                    {
                        var agentData = AgentData.Parse(line);
                        if (agentData != null)
                        {
                            aggregator.Aggregate(agentData);
                            processedLines++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{_agentName}] Error parsing data: {ex.Message}");
                        Console.WriteLine($"[{_agentName}] Problematic line: {line}");
                    }
                }

                Console.WriteLine($"[{_agentName}] Disconnected before sending END_OF_DATA.");
            }
            catch (IOException)
            {
                Console.WriteLine($"[{_agentName}] Disconnected unexpectedly.");
            }
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
            _streamReader = null;

            _pipeServer?.Dispose();
            _pipeServer = null;
            Console.WriteLine($"[{_agentName}] Connection closed.");
            GC.SuppressFinalize(this);
        }
    }
}
