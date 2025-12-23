using System;

namespace CollectionsPerformance
{
    static class CollectionBenchmarkRunner
    {
        public static void RunAllBenchmarks(int collectionSize = 100000, int iterations = 5)
        {
            Console.WriteLine("Анализ производительности коллекций");
            
            Console.WriteLine("Тестирование List<int>");
            ListBenchmark.TestPerformance(collectionSize, iterations);
            
            Console.WriteLine("Тестирование LinkedList<int>");
            LinkedListBenchmark.TestPerformance(collectionSize, iterations);
            
            Console.WriteLine("Тестирование Queue<int>");
            QueueBenchmark.TestPerformance(collectionSize, iterations);
            
            Console.WriteLine("Тестирование Stack<int>");
            StackBenchmark.TestPerformance(collectionSize, iterations);
            
            Console.WriteLine("Тестирование ImmutableList<int>");
            ImmutableListBenchmark.TestPerformance(collectionSize, iterations);
            
            Console.WriteLine("Тестирование завершено");
        }
    }
}

