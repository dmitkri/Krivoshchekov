using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace CollectionsPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Анализ производительности коллекций ===\n");
            
            const int collectionSize = 100000;
            const int iterations = 5;
            
            Console.WriteLine("Тестирование List<int>...");
            TestListPerformance(collectionSize, iterations);
            
            Console.WriteLine("\nТестирование LinkedList<int>...");
            TestLinkedListPerformance(collectionSize, iterations);
            
            Console.WriteLine("\nТестирование Queue<int>...");
            TestQueuePerformance(collectionSize, iterations);
            
            Console.WriteLine("\nТестирование Stack<int>...");
            TestStackPerformance(collectionSize, iterations);
            
            Console.WriteLine("\nТестирование ImmutableList<int>...");
            TestImmutableListPerformance(collectionSize, iterations);
            
            Console.WriteLine("\n=== Тестирование завершено ===");
        }
        
        static void TestListPerformance(int size, int iterations)
        {
            var results = new PerformanceResults("List<int>");
            const int fastOpIterations = 10000; // Для быстрых операций
            
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
        
        static void TestLinkedListPerformance(int size, int iterations)
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
        
        static void TestQueuePerformance(int size, int iterations)
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
        
        static void TestStackPerformance(int size, int iterations)
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
        
        static void TestImmutableListPerformance(int size, int iterations)
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
    
    class PerformanceResults
    {
        public string CollectionName { get; }
        public List<double> AddToEnd { get; }
        public List<double> AddToStart { get; }
        public List<double> AddToMiddle { get; }
        public List<double> Search { get; }
        public List<double> GetByIndex { get; }
        public List<double> RemoveFromStart { get; }
        public List<double> RemoveFromEnd { get; }
        public List<double> RemoveFromMiddle { get; }
        
        public PerformanceResults(string collectionName)
        {
            CollectionName = collectionName;
            AddToEnd = new List<double>();
            AddToStart = new List<double>();
            AddToMiddle = new List<double>();
            Search = new List<double>();
            GetByIndex = new List<double>();
            RemoveFromStart = new List<double>();
            RemoveFromEnd = new List<double>();
            RemoveFromMiddle = new List<double>();
        }
        
        public void PrintResults()
        {
            Console.WriteLine($"Результаты для {CollectionName}:");
            Console.WriteLine($"  Добавление в конец:     {CalculateAverage(AddToEnd):F3} мс");
            Console.WriteLine($"  Добавление в начало:    {CalculateAverage(AddToStart):F3} мс");
            Console.WriteLine($"  Добавление в середину:  {CalculateAverage(AddToMiddle):F3} мс");
            Console.WriteLine($"  Поиск элемента:         {CalculateAverage(Search):F3} мс");
            Console.WriteLine($"  Получение по индексу:   {CalculateAverage(GetByIndex):F3} мс");
            Console.WriteLine($"  Удаление из начала:     {CalculateAverage(RemoveFromStart):F3} мс");
            Console.WriteLine($"  Удаление из конца:      {CalculateAverage(RemoveFromEnd):F3} мс");
            Console.WriteLine($"  Удаление из середины:  {CalculateAverage(RemoveFromMiddle):F3} мс");
        }
        
        private double CalculateAverage(List<double> values)
        {
            if (values.Count == 0)
                return 0;
            return values.Where(v => v > 0).DefaultIfEmpty(0).Average();
        }
    }
}

