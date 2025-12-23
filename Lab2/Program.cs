namespace CollectionsPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            const int collectionSize = 100000;
            const int iterations = 5;
            
            CollectionBenchmarkRunner.RunAllBenchmarks(collectionSize, iterations);
        }
    }
}

