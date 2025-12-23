using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CollectionsPerformance
{
    static class StackBenchmark
    {
        public static void TestPerformance(int size, int iterations)
        {
            var results = new PerformanceResults("Stack<int>");
            const int fastOpIterations = 10000;
            
            for (int i = 0; i < iterations; i++)
            {
                var stack = new Stack<int>();
                var sw = Stopwatch.StartNew();
                
                sw.Restart();
                for (int j = 0; j < size; j++)
                {
                    stack.Push(j);
                }
                results.AddToStart.Add(sw.Elapsed.TotalMilliseconds);
                
                results.AddToEnd.Add(0);
                results.AddToMiddle.Add(0);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    bool found = stack.Contains(size / 2);
                }
                results.Search.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                results.GetByIndex.Add(0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    stack.Pop();
                }
                results.RemoveFromStart.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                results.RemoveFromEnd.Add(0);
                results.RemoveFromMiddle.Add(0);
            }
            
            results.PrintResults();
        }
    }
}

