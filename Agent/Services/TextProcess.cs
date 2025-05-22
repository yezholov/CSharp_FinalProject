namespace Agent.Services;

public class TextProcessor
{
    // Separators for splitting the line into words (for splitting the line into words)
    private readonly char[] _separators = [' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '(', ')', '-'];
    
    // Process the line into words 
    public IEnumerable<string> ProcessLine(string line)
    {
        if (string.IsNullOrEmpty(line)) // Check if the line is empty
            return Enumerable.Empty<string>(); // Return an empty collection
            
        return line.Split(_separators, StringSplitOptions.RemoveEmptyEntries) // Split the line into words
                  .Select(word => word.ToLower()) // Normalize to lower case
                  .Where(word => !string.IsNullOrWhiteSpace(word)); // Delete empty words
    }
}
