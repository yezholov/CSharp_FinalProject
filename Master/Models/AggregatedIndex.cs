namespace Master.Models
{
    public class AggregatedIndex
    {
        private readonly List<AgentData> _allEntries = new();

        public void AddEntry(AgentData data)
        {
            _allEntries.Add(data);
        }

        public IEnumerable<AgentData> GetSortedEntries()
        {
            return _allEntries
                .OrderBy(entry => entry.FileName) // sort by FileName (A-Z)
                .ThenByDescending(entry => entry.Count) // sort by Count (DESC)
                .ThenBy(entry => entry.Word); // sort by Word (A-Z)
        }

        public bool HasData() => _allEntries.Count > 0;
    }
}
