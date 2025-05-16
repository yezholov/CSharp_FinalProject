using Agent.Helpers;
using Agent.Services;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide directory path as argument");
            return;
        }

        string directoryPath = FileHelper.NormalizePath(args[0]);
        var scanner = new FileScanner();

        try
        {
            var files = await scanner.GetTextFilesAsync(directoryPath);
            Console.WriteLine($"Found {files.Count} text files");

            foreach (var file in files)
            {
                var result = await scanner.ScanFileAsync(file);
                // Now only print the file name and the words with their counts
                Console.WriteLine($"\nFile: {result.FileName}");
                foreach (var word in result.Words)
                {
                    Console.WriteLine($"{word.Word}: {word.Count}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
