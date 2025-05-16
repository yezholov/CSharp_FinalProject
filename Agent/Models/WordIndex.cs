namespace Agent.Models;

public class WordIndex(string word, int count, string fileName)
{
    public string Word = word;
    public int Count = count;
    public string FileName = fileName;
}