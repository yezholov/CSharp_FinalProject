using System.IO.Pipes;
using Agent.Models;

namespace Agent.Services
{
    public class PipeClient : IDisposable
    {
        private NamedPipeClientStream? _pipeClient;
        private StreamWriter? _streamWriter;
        private StreamReader? _streamReader;
        private readonly string _pipeName;
        private readonly string _serverName = "."; // Represents the local machine

        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public PipeClient(string pipeName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                throw new ArgumentNullException(nameof(pipeName));
            _pipeName = pipeName;
        }

        public async Task ConnectAsync(int timeoutMilliseconds = 5000)
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(
                    _serverName,
                    _pipeName,
                    PipeDirection.InOut, // for two-way communication
                    PipeOptions.Asynchronous
                );

                await _pipeClient.ConnectAsync(timeoutMilliseconds);

                if (_pipeClient.IsConnected)
                {
                    _streamWriter = new StreamWriter(_pipeClient) { AutoFlush = true };
                    _streamReader = new StreamReader(_pipeClient); // Initialize StreamReader
                    Console.WriteLine($"Connected to Master on pipe: '{_pipeName}' (InOut)");
                }
                else
                {
                    Console.WriteLine(
                        $"Failed to connect to Master on pipe: '{_pipeName}' within the timeout period."
                    );
                    _pipeClient?.Dispose();
                    _pipeClient = null;
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Connection to Master on pipe '{_pipeName}' timed out.");
                _pipeClient?.Dispose();
                _pipeClient = null;
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error connecting to Master on pipe '{_pipeName}': {ex.Message}"
                );
                _pipeClient?.Dispose();
                _pipeClient = null;
                throw;
            }
        }

        public async Task SendDataAsync(FileIndex fileIndexData)
        {
            if (!IsConnected || _streamWriter == null)
            {
                throw new InvalidOperationException(
                    "Client is not connected. Call ConnectAsync first."
                );
            }

            if (fileIndexData == null || fileIndexData.Words == null)
            {
                Console.WriteLine("No data to send for FileIndex.");
                return;
            }

            try
            {
                foreach (var wordIndex in fileIndexData.Words)
                {
                    string message = $"{wordIndex.FileName}:{wordIndex.Word}:{wordIndex.Count}";
                    await _streamWriter.WriteLineAsync(message);
                }
                // Send end of data marker
                await _streamWriter.WriteLineAsync("END_OF_FILE_DATA");
            }
            catch (IOException ex)
            {
                Console.WriteLine(
                    $"IO Error sending data for {fileIndexData.FileName}: {ex.Message}"
                );
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data for {fileIndexData.FileName}: {ex.Message}");
                throw;
            }
        }

        public async Task<string?> ReceiveCommandAsync()
        {
            if (!IsConnected || _streamReader == null)
            {
                throw new InvalidOperationException(
                    "Client is not connected or reader is not available. Call ConnectAsync first."
                );
            }

            try
            {
                return await _streamReader.ReadLineAsync();
            }
            catch (IOException ex)
            {
                Console.WriteLine(
                    $"IO Error receiving command on pipe '{_pipeName}': {ex.Message}"
                );
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving command on pipe '{_pipeName}': {ex.Message}");
                throw;
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                _streamWriter?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing StreamWriter: {ex.Message}");
            }
            _streamWriter = null;

            try
            {
                _streamReader?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing StreamReader: {ex.Message}");
            }
            _streamReader = null;

            try
            {
                _pipeClient?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing NamedPipeClientStream: {ex.Message}");
            }
            _pipeClient = null;
            Console.WriteLine($"PipeClient for '{_pipeName}' disposed.");
        }
    }
}
