using Master.Models;

namespace Master.Services
{
    public class ResultPrinter
    {
        /*
        This method is used to print the aggregated data to the console.
        Group files and sort by file name.
        Then print the word and the count.
        */
        public void PrintToConsole(AggregatedIndex aggregatedIndex)
        {
            Console.WriteLine("\n--- Start of Report ---");
            // If there is no data to display, write and return
            if (aggregatedIndex == null || !aggregatedIndex.HasData())
            {
                Console.WriteLine("No data to display.");
                Console.WriteLine("--- End of Report ---\n");
                return;
            }

            // Group the data by file name and sort by file name
            var groupedByFile = aggregatedIndex
                .GetSortedEntries()
                .GroupBy(entry => entry.FileName)
                .OrderBy(entry => entry.Key);

            foreach (var fileGroup in groupedByFile) // For each file
            {
                Console.WriteLine($"File: {fileGroup.Key}");
                Console.WriteLine("  Word                Count");
                Console.WriteLine("  ------------------- -----");
                foreach (var entry in fileGroup) // For each word in the file
                {
                    Console.WriteLine($"  {entry.Word, -20} {entry.Count, 5}");
                }
                Console.WriteLine();
            }
            Console.WriteLine("--- End of Report ---\n");
        }
    }
}
