namespace Master.Models
{
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

        public static AgentData Parse(string dataLine)
        {
            var parts = dataLine.Split(':');
            return new AgentData(parts[0], parts[1], int.Parse(parts[2]));
        }

        public override string ToString()
        {
            return $"{FileName}:{Word}:{Count}";
        }
    }
}
