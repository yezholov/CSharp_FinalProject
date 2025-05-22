namespace Master.Models
{
    /*
    This class is used to store the data from the agents.
    Agent data(the same WordIndex):
        - FileName (string): The name of the file.
        - Word (string): The word to count.
        - Count (int): The number of times the word appears in the file.
    */
    public class AgentData
    {
        public string FileName { get; private set; }
        public string Word { get; private set; }
        public int Count { get; private set; }

        private AgentData(string fileName, string word, int count)
        {
            FileName = fileName;
            Word = word;
            Count = count;
        }

        /*
        Parse the data from the string
        Input: "FileName:Word:Count"
        Output: AgentData object
        */
        public static AgentData Parse(string dataLine)
        {
            var parts = dataLine.Split(':');
            return new AgentData(parts[0], parts[1], int.Parse(parts[2]));
        }
    }
}
