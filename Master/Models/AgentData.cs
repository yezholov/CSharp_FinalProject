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
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Word = word ?? throw new ArgumentNullException(nameof(word));
            Count = count;
        }

        /*
        Parse the data from the string
        Input: "FileName:Word:Count"
        Output: AgentData object
        Throws ArgumentException if the input format is invalid
        */
        public static AgentData Parse(string dataLine)
        {
            if (string.IsNullOrEmpty(dataLine))
                throw new ArgumentException("Input string cannot be null or empty", nameof(dataLine));

            var parts = dataLine.Split(':');
            if (parts.Length != 3)
                throw new ArgumentException($"Invalid format. Expected 'FileName:Word:Count', got '{dataLine}'", nameof(dataLine));

            if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                throw new ArgumentException($"FileName and Word cannot be empty in '{dataLine}'", nameof(dataLine));

            if (!int.TryParse(parts[2], out int count))
                throw new ArgumentException($"Invalid count value in '{dataLine}'", nameof(dataLine));

            return new AgentData(parts[0], parts[1], count);
        }
    }
}
