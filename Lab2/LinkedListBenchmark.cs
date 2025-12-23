using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CollectionsPerformance
{
    static class LinkedListBenchmark
    {
        public static void TestPerformance(int size, int iterations)
        {
            var results = new PerformanceResults("LinkedList<int>");
            const int fastOpIterations = 1000;
            
            for (int i = 0; i < iterations; i++)
            {
                var linkedList = new LinkedList<int>();
                var sw = Stopwatch.StartNew();
                
                sw.Restart();
                for (int j = 0; j < size; j++)
                {
                    linkedList.AddLast(j);
                }
                results.AddToEnd.Add(sw.Elapsed.TotalMilliseconds);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    linkedList.AddFirst(-1);
                }
                results.AddToStart.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    var middleNode = linkedList.First;
                    for (int k = 0; k < linkedList.Count / 2 && middleNode != null; k++)
                    {
                        middleNode = middleNode.Next;
                    }
                    if (middleNode != null)
                    {
                        linkedList.AddAfter(middleNode, -2);
                    }
                }
                results.AddToMiddle.Add(sw.Elapsed.TotalMilliseconds / 100.0);
                
                sw.Restart();
                for (int j = 0; j < fastOpIterations; j++)
                {
                    bool found = linkedList.Contains(size / 2);
                }
                results.Search.Add(sw.Elapsed.TotalMilliseconds / fastOpIterations);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    int index = 0;
                    int value = 0;
                    foreach (var item in linkedList)
                    {
                        if (index == linkedList.Count / 2)
                        {
                            value = item;
                            break;
                        }
                        index++;
                    }
                }
                results.GetByIndex.Add(sw.Elapsed.TotalMilliseconds / 100.0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    linkedList.RemoveFirst();
                }
                results.RemoveFromStart.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < 1000; j++)
                {
                    linkedList.RemoveLast();
                }
                results.RemoveFromEnd.Add(sw.Elapsed.TotalMilliseconds / 1000.0);
                
                sw.Restart();
                for (int j = 0; j < 100; j++)
                {
                    var middleNode = linkedList.First;
                    for (int k = 0; k < linkedList.Count / 2 && middleNode != null; k++)
                    {
                        middleNode = middleNode.Next;
                    }
                    if (middleNode != null)
                    {
                        linkedList.Remove(middleNode);
                    }
                }
                results.RemoveFromMiddle.Add(sw.Elapsed.TotalMilliseconds / 100.0);
            }
            
            results.PrintResults();
        }
    }
}

