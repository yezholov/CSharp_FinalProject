namespace Agent.Services;

using Agent.Models;

public class FileScanner
{
    private readonly TextProcessor _textProcessor; // Text processor

    public FileScanner()
    {
        _textProcessor = new TextProcessor();
    }

    /*
        Scan a file and return a FileIndex object
    */
    public async Task<FileIndex> ScanFileAsync(string filePath)
    {
        var result = new FileIndex(Path.GetFileName(filePath)); // Create a new index

        try
        {
            using var reader = new StreamReader(filePath); // Create a new stream reader
            string? line;
            while ((line = await reader.ReadLineAsync()) is not null) // Read the file line by line
            {
                var lineWords = _textProcessor.ProcessLine(line); // Process the line into words
                foreach (var word in lineWords) // For each word
                {
                    result.AddWord(word); // Add the word to the index
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
        }

        return result;
    }

    public async Task<List<string>> GetTextFilesAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath)) // Check if the directory exists
        {
            throw new DirectoryNotFoundException($"Error: Directory not found: {directoryPath}");
        }

        return await Task.Run(() => Directory.GetFiles(directoryPath, "*.txt").ToList()); // Get the txt files in the directory and return a list
    }
}
