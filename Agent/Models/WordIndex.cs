namespace Agent.Models;

/*
    This class is used to store the index of a word in a file.
    Word index(the same AgentData):
        - FileName (string): The name of the file.
        - Word (string): The word to count.
        - Count (int): The number of times the word appears in the file.
*/
public class WordIndex(string word, int count, string fileName)
{
    public string Word = word;
    public int Count = count;
    public string FileName = fileName;

    /*
        Convert the word index to a string
        Input: WordIndex objjtect
        Output: "FileName:Word:Count"
    */
    public override string ToString()
    {
        return $"{FileName}:{Word}:{Count}";
    }
}