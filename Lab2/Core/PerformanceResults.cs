using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionsPerformance
{
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

