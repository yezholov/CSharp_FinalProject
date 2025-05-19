using System.IO.Pipes;
using Agent.Models;

namespace Agent.Services
{
    public class PipeClient : IDisposable
    {
        private NamedPipeClientStream? _pipeClient;
        private StreamWriter? _streamWriter;
        private readonly string _pipeName;
        private readonly string _serverName = "."; // Represents the local machine
        private readonly string _agentName;

        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public PipeClient(string pipeName, string agentName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                throw new ArgumentNullException(nameof(pipeName));
            _pipeName = pipeName;
            _agentName = agentName;
        }

        public async Task ConnectAsync(int timeoutMilliseconds = 5000)
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(
                    _serverName,
                    _pipeName,
                    PipeDirection.Out,
                    PipeOptions.Asynchronous
                );

                await _pipeClient.ConnectAsync(timeoutMilliseconds);

                if (_pipeClient.IsConnected)
                {
                    _streamWriter = new StreamWriter(_pipeClient) { AutoFlush = true };
                }
                else
                {
                    Console.WriteLine(
                        $"[{_agentName}] Failed to connect to Master within timeout period."
                    );
                    _pipeClient?.Dispose();
                    _pipeClient = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_agentName}] Connection error: {ex.Message}");
                _pipeClient?.Dispose();
                _pipeClient = null;
                throw;
            }
        }

        public async Task SendDataAsync(FileIndex fileIndexData)
        {
            if (!IsConnected || _streamWriter == null)
            {
                throw new InvalidOperationException($"[{_agentName}] Not connected to Master.");
            }

            if (fileIndexData == null || fileIndexData.Words == null)
            {
                Console.WriteLine($"[{_agentName}] No data to send.");
                return;
            }

            try
            {
                foreach (var wordIndex in fileIndexData.Words)
                {
                    await _streamWriter.WriteLineAsync(wordIndex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_agentName}] Error sending data: {ex.Message}");
                throw;
            }
        }

        public async Task SendEndOfDataMarker()
        {
            if (!IsConnected || _streamWriter == null)
            {
                throw new InvalidOperationException($"[{_agentName}] Not connected to Master.");
            }

            try
            {
                await _streamWriter.WriteLineAsync("END_OF_DATA");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_agentName}] Error sending end marker: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
            _streamWriter = null;

            _pipeClient?.Dispose();
            _pipeClient = null;
            Console.WriteLine($"[{_agentName}] Disconnected from Master.");
        }
    }
}
