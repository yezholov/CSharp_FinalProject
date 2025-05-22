using System.IO.Pipes;
using Master.Models;

namespace Master.Services
{
    public class PipeServer : IDisposable
    {
        private NamedPipeServerStream? _pipeServer; // Pipe server
        private StreamReader? _streamReader; // Stream reader
        private readonly string _pipeName; // Agent pipe name (name for connection)
        private readonly string _agentName; // Agent name (name of the agent for logging)
        private Logger _logger; // Logger (with pre-created name and color)

        public bool IsClientConnected => _pipeServer?.IsConnected ?? false;

        public PipeServer(string pipeName, string agentName, Logger logger)
        {
            if (string.IsNullOrWhiteSpace(pipeName)) // Check if the pipe name is valid
                throw new ArgumentNullException(nameof(pipeName));
            _pipeName = pipeName;
            _agentName = agentName;
            _logger = logger;
        }

        public async Task WaitForConnectionAsync(int timeout = 10000)
        {
            try
            {
                _pipeServer?.Dispose(); // For safety, dispose the pipe server if it exists
                _pipeServer = new NamedPipeServerStream(
                    _pipeName, // Pipe name (name for connection)
                    PipeDirection.In // Pipe direction (input)
                );

                _logger.Log($"Pipe server '{_pipeName}' created. Waiting for agent connection...");
                await _pipeServer.WaitForConnectionAsync(); // Forwarding functionallity to the NamedPipeServerStream object

                if (_pipeServer.IsConnected) // Check if the connection is successful
                {
                    _streamReader = new StreamReader(_pipeServer); // Create a stream reader
                    _logger.Log($"Connected.");
                }
                else
                {
                    _logger.Error($"Connection failed after waiting.");
                    Dispose(); // For safety, dispose the pipe server if we can't connect
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error connecting: {ex.Message}");
                Dispose(); // For safety, dispose the pipe server if we get an error
                throw;
            }
        }

        public async Task ReceiveDataAsync(DataAggregator aggregator)
        {
            //Check if the client is connected and the stream reader created
            if (!IsClientConnected || _streamReader == null)
            {
                throw new InvalidOperationException($"{_agentName} is not connected.");
            }

            try
            {
                _logger.Log($"Receiving data...");
                string? line;
                int processedLines = 0;

                while ((line = await _streamReader.ReadLineAsync()) != null) // Read the data from the stream
                {
                    if (line.Equals("END_OF_DATA", StringComparison.OrdinalIgnoreCase)) // Check if the data is the end of the data
                    {
                        _logger.Log(
                            $"Finished receiving data. Processed {processedLines} entries."
                        );
                        return;
                    }

                    try
                    {
                        var agentData = AgentData.Parse(line); // Parse the data
                        if (agentData != null)
                        {
                            aggregator.Aggregate(agentData); // Aggregate the data
                            processedLines++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(
                            $"Error parsing data: {ex.Message}\nProblematic line: {line}"
                        );
                    }
                }

                _logger.Error($"Disconnected before sending END_OF_DATA.");
            }
            catch (IOException)
            {
                _logger.Error($"Disconnected unexpectedly.");
            }
        }

        public void Dispose()
        {
            if (_streamReader != null)
            {
                _streamReader.Dispose();
                _streamReader = null;
            }

            _pipeServer?.Dispose(); // Forwarding functionallity to the NamedPipeServerStream object (Dispose the pipe server)
            _pipeServer = null;
            _logger.Log($"Connection closed.");
            GC.SuppressFinalize(this); // For safety, suppress the destructor
        }
    }
}
