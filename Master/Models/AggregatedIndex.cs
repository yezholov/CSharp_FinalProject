namespace Master.Models
{
    /*
    This class is used to store the aggregated data from the agents.
    It uses a list to store the data from the agents.
    */
    public class AggregatedIndex
    {
        //List of AgentData objects
        private readonly List<AgentData> _allEntries = new();

        // Add the data to the end of the list
        public void AddEntry(AgentData data)
        {
            _allEntries.Add(data);
        }

        public IEnumerable<AgentData> GetSortedEntries()
        {
            // Sort the list by FileName (A-Z), then by Count (DESC), then by Word (A-Z)
            return _allEntries
                .OrderBy(entry => entry.FileName) // sort by FileName (A-Z)
                .ThenByDescending(entry => entry.Count) // sort by Count (DESC)
                .ThenBy(entry => entry.Word); // sort by Word (A-Z)
        }

        // Check if the list has data
        public bool HasData() => _allEntries.Count > 0;
    }
}
