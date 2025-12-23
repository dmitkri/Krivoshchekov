using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CollectionsPerformance
{
    static class QueueBenchmark
    {
        public static void TestPerformance(int size, int iterations)
        {
            var results = new PerformanceResults("Queue<int>");
            const int fastOpIterations = 10000;
            
            for (int i = 0; i < iterations; i++)
            {
                var queue = new Queue<int>();
                var sw = Stopwatch.StartNew();
                
                sw.Restart();
                for (int j = 0; j < size; j++)
                {
                    queue.Enqueue(j);
                }
                results.AddToEnd.Add(sw.Elapsed.TotalMilliseconds);
                
                results.AddToStart.Add(0);
                results.AddToMiddle.Add(0);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    bool found = queue.Contains(size / 2);
                }
                results.Search.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                results.GetByIndex.Add(0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    queue.Dequeue();
                }
                results.RemoveFromStart.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                results.RemoveFromEnd.Add(0);
                results.RemoveFromMiddle.Add(0);
            }
            
            results.PrintResults();
        }
    }
}

