using Master.Models;

namespace Master.Services
{
    /*
    This class is used to aggregate the data from the agents.
    It uses a list of AgentData objects to store the data.
    */
    public class DataAggregator
    {
        private readonly AggregatedIndex _aggregatedIndex = new();

        public void Aggregate(AgentData data)
        {
            _aggregatedIndex.AddEntry(data);
        }

        public AggregatedIndex GetResults()
        {
            return _aggregatedIndex;
        }
    }
}
