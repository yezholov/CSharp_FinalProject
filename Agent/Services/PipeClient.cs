using System.IO.Pipes;
using Agent.Models;

namespace Agent.Services
{
    public class PipeClient : IDisposable
    {
        private NamedPipeClientStream? _pipeClient; // Pipe client
        private StreamWriter? _streamWriter; // Stream writer
        private readonly string _pipeName; // Pipe name (name for connection)
        private readonly string _serverName = "."; // Represents the local machine
        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public PipeClient(string pipeName, string agentName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                throw new ArgumentNullException(nameof(pipeName));
            _pipeName = pipeName;
        }

        //Connect to the master
        public async Task ConnectAsync(int timeoutMilliseconds = 5000)
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(
                    _serverName, // Server name (local machine)
                    _pipeName, // Pipe name (name for connection)
                    PipeDirection.Out, // Pipe direction (output)
                    PipeOptions.Asynchronous // Pipe options (asynchronous)
                );

                await _pipeClient.ConnectAsync(timeoutMilliseconds); //Forwarding functionallity to the NamedPipeClientStream object

                if (_pipeClient.IsConnected) // Check if the connection is successful
                {
                    _streamWriter = new StreamWriter(_pipeClient) { AutoFlush = true }; // Create a stream writer
                }
                else
                {
                    Console.WriteLine($"Error: Failed to connect to Master within timeout period.");
                    Dispose(); // For safety, dispose the pipe client if we can't connect
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Connecting to Master {ex.Message}");
                Dispose(); // For safety, dispose the pipe client if we get an error
                throw;
            }
        }

        public async Task SendDataAsync(FileIndex fileIndexData)
        {
            //Check if the client is connected and the stream writer created
            if (!IsConnected || _streamWriter == null)
            {
                throw new InvalidOperationException($"Not connected to Master.");
            }

            //Check if we have data to send
            if (fileIndexData == null || fileIndexData.Words == null)
            {
                Console.WriteLine($"No data to send.");
                await SendEndOfDataMarker(); // Send the end of data marker
                return;
            }

            try
            {
                foreach (var wordIndex in fileIndexData.Words) // For each index (word) in the file
                {
                    await _streamWriter.WriteLineAsync(wordIndex.ToString()); // Send the index to the master
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Sending data: {ex.Message}");
                throw;
            }
        }

        public async Task SendEndOfDataMarker()
        {
            //Check if the client is connected and the stream writer created
            if (!IsConnected || _streamWriter == null)
            {
                throw new InvalidOperationException($"Not connected to Master.");
            }

            try
            {
                await _streamWriter.WriteLineAsync("END_OF_DATA"); // Send the end of data marker
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Sending end marker: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Dispose();
                _streamWriter = null;
            }

            _pipeClient?.Dispose(); //Forwarding functionallity to the NamedPipeClientStream object
            _pipeClient = null;
            Console.WriteLine($"Disconnected from Master.");
            GC.SuppressFinalize(this); // For safety, suppress the destructor
        }
    }
}
