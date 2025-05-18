using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Collections.Generic; // For List<string>

namespace Master.Services
{
    public class PipeServer : IDisposable
    {
        private NamedPipeServerStream? _pipeServer;
        private StreamWriter? _streamWriter;
        private StreamReader? _streamReader;
        private readonly string _pipeName;

        public bool IsClientConnected => _pipeServer?.IsConnected ?? false;

        public PipeServer(string pipeName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                throw new ArgumentNullException(nameof(pipeName));
            _pipeName = pipeName;
        }

        public async Task WaitForConnectionAsync()
        {
            try
            {
                _pipeServer?.Dispose();

                _pipeServer = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut // for two-way communication
                );

                Console.WriteLine(
                    $"Pipe server '{_pipeName}' created. Waiting for agent connection..."
                );
                await _pipeServer.WaitForConnectionAsync();

                if (_pipeServer.IsConnected)
                {
                    _streamWriter = new StreamWriter(_pipeServer) { AutoFlush = true };
                    _streamReader = new StreamReader(_pipeServer);
                    Console.WriteLine($"Agent connected to pipe '{_pipeName}'.");
                }
                else
                {
                    Console.WriteLine(
                        $"Agent connection to pipe '{_pipeName}' failed after WaitForConnectionAsync."
                    );
                    _pipeServer?.Dispose();
                    _pipeServer = null;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(
                    $"IOException on pipe '{_pipeName}' during connection: {ex.Message}. This might happen if the pipe name is already in use or access is denied."
                );
                _pipeServer?.Dispose();
                _pipeServer = null;
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error waiting for connection on pipe '{_pipeName}': {ex.Message}"
                );
                _pipeServer?.Dispose();
                _pipeServer = null;
                throw;
            }
        }

        public async Task SendCommandAsync(string command)
        {
            if (!IsClientConnected || _streamWriter == null)
            {
                throw new InvalidOperationException(
                    "Client is not connected. Call WaitForConnectionAsync first."
                );
            }
            if (string.IsNullOrEmpty(command))
            {
                Console.WriteLine("Cannot send an empty command.");
                return;
            }

            try
            {
                await _streamWriter.WriteLineAsync(command);
                Console.WriteLine($"Sent command '{command}' to agent on pipe '{_pipeName}'.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Error sending command on pipe '{_pipeName}': {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> ReceiveDataAsync()
        {
            if (!IsClientConnected || _streamReader == null)
            {
                throw new InvalidOperationException(
                    "Client is not connected. Call WaitForConnectionAsync first."
                );
            }

            var receivedLines = new List<string>();
            string? line;
            Console.WriteLine($"Waiting to receive data from agent on pipe '{_pipeName}'...");
            try
            {
                while ((line = await _streamReader.ReadLineAsync()) != null)
                {
                    if (line.Equals("END_OF_FILE_DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        // Stop reading when marker is received
                        break;
                    }
                    receivedLines.Add(line);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Error receiving data on pipe '{_pipeName}': {ex.Message}");
                throw;
            }
            Console.WriteLine($"Finished receiving data. Total lines: {receivedLines.Count}");
            return receivedLines;
        }

        public void CloseClientConnection()
        {
            Console.WriteLine($"Closing client connection for pipe '{_pipeName}'.");
            _streamWriter?.Dispose();
            _streamWriter = null;
            _streamReader?.Dispose();
            _streamReader = null;

            _pipeServer?.Dispose();
            _pipeServer = null;
        }

        public void Dispose()
        {
            CloseClientConnection();
            Console.WriteLine($"PipeServer for '{_pipeName}' disposed.");
            GC.SuppressFinalize(this);
        }
    }
}
