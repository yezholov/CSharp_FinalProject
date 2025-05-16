namespace Agent.Services;

public class TextProcessor
{
    private readonly char[] _separators = [' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '(', ')', '-'];
    
    public IEnumerable<string> ProcessLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return Enumerable.Empty<string>();
            
        return line.Split(_separators, StringSplitOptions.RemoveEmptyEntries)
                  .Select(word => word.ToLower())
                  .Where(word => !string.IsNullOrWhiteSpace(word));
    }
}
