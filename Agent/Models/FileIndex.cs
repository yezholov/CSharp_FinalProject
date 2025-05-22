namespace Agent.Models;

/*
    This class is used to store the index of a file
    It contains the file name and a list of word indexes
*/
public class FileIndex(string fileName)
{
    public string FileName = fileName;
    public List<WordIndex> Words = [];

    public void AddWord(string word)
    {
        var existingWord = Words.FirstOrDefault(w => w.Word == word); // Check if the word already exists
        if (existingWord != null) // If the word exists, increment the count
            existingWord.Count++;
        else // If the word does not exist, add it to the list
            Words.Add(new WordIndex(word, 1, FileName));
    }
}
