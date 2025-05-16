namespace Agent.Helpers;

public static class FileHelper
{
    public static bool IsFileAccessible(string filePath)
    {
        try
        {
            using FileStream stream = File.Open(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None
            );
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.Trim().Replace("\"", ""));
    }
}
