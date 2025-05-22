namespace Agent.Helpers;

public static class FileHelper
{
    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.Trim().Replace("\"", "")); // Normalize the path
    }
}
