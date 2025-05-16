namespace Agent.Services;

using Agent.Models;

public class FileScanner
{
    private readonly TextProcessor _textProcessor;

    public FileScanner()
    {
        _textProcessor = new TextProcessor();
    }

    public async Task<FileIndex> ScanFileAsync(string filePath)
    {
        var result = new FileIndex(Path.GetFileName(filePath));

        try
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                var lineWords = _textProcessor.ProcessLine(line);
                foreach (var word in lineWords)
                {
                    result.AddWord(word);
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
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        return await Task.Run(() => Directory.GetFiles(directoryPath, "*.txt").ToList());
    }
}
