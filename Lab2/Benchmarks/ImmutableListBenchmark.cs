using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CollectionsPerformance
{
    static class ImmutableListBenchmark
    {
        public static void TestPerformance(int size, int iterations)
        {
            var results = new PerformanceResults("ImmutableList<int>");
            const int fastOpIterations = 10000;
            
            for (int i = 0; i < iterations; i++)
            {
                var immutableList = ImmutableList<int>.Empty;
                var sw = Stopwatch.StartNew();
                
                sw.Restart();
                for (int j = 0; j < size; j++)
                {
                    immutableList = immutableList.Add(j);
                }
                results.AddToEnd.Add(sw.Elapsed.TotalMilliseconds);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    immutableList = immutableList.Insert(0, -1);
                }
                results.AddToStart.Add(sw.Elapsed.TotalMilliseconds / 100.0);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    immutableList = immutableList.Insert(immutableList.Count / 2, -2);
                }
                results.AddToMiddle.Add(sw.Elapsed.TotalMilliseconds / 100.0);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    bool found = immutableList.Contains(size / 2);
                }
                results.Search.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    int value = immutableList[immutableList.Count / 2];
                }
                results.GetByIndex.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    immutableList = immutableList.RemoveAt(0);
                }
                results.RemoveFromStart.Add(sw.Elapsed.TotalMilliseconds / 100.0);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    immutableList = immutableList.RemoveAt(immutableList.Count - 1);
                }
                results.RemoveFromEnd.Add(sw.Elapsed.TotalMilliseconds / 100.0);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    immutableList = immutableList.RemoveAt(immutableList.Count / 2);
                }
                results.RemoveFromMiddle.Add(sw.Elapsed.TotalMilliseconds / 100.0);
            }
            
            results.PrintResults();
        }
    }
}

