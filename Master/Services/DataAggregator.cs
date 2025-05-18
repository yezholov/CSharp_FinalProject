using Master.Models;

namespace Master.Services
{
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
