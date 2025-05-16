namespace Agent.Models;

public class FileIndex(string fileName)
{
    public string FileName = fileName;
    public List<WordIndex> Words = [];

    public void AddWord(string word)
    {
        var existingWord = Words.FirstOrDefault(w => w.Word == word);
        if (existingWord != null)
            existingWord.Count++;
        else
            Words.Add(new WordIndex(word, 1, FileName));
    }
}
