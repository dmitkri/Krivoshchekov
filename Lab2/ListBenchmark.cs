using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CollectionsPerformance
{
    static class ListBenchmark
    {
        public static void TestPerformance(int size, int iterations)
        {
            var results = new PerformanceResults("List<int>");
            const int fastOpIterations = 10000;
            
            for (int i = 0; i < iterations; i++)
            {
                var list = new List<int>();
                var sw = Stopwatch.StartNew();
                
                sw.Restart();
                for (int j = 0; j < size; j++)
                {
                    list.Add(j);
                }
                results.AddToEnd.Add(sw.Elapsed.TotalMilliseconds);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    list.Insert(0, -1);
                }
                results.AddToStart.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    list.Insert(list.Count / 2, -2);
                }
                results.AddToMiddle.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    bool found = list.Contains(size / 2);
                }
                results.Search.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    int value = list[size / 2];
                }
                results.GetByIndex.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    list.RemoveAt(0);
                }
                results.RemoveFromStart.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    list.RemoveAt(list.Count - 1);
                }
                results.RemoveFromEnd.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    list.RemoveAt(list.Count / 2);
                }
                results.RemoveFromMiddle.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
            }
            
            results.PrintResults();
        }
    }
}

