using lab3.Collections;

namespace lab3;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Тестирование SimpleList");
        var myList = new SimpleList<int>();
        myList.Add(10);
        myList.Add(20);
        myList.Add(30);
        Console.WriteLine($"В списке элементов: {myList.Count}");
        
        foreach (var element in myList)
        {
            Console.WriteLine($"Элемент: {element}");
        }

        Console.WriteLine();

        Console.WriteLine("Тестирование SimpleDictionary");
        var myDict = new SimpleDictionary<string, int>();
        myDict.Add("первый", 1);
        myDict.Add("второй", 2);
        myDict.Add("третий", 3);
        Console.WriteLine($"В словаре элементов: {myDict.Count}");
        
        foreach (var pair in myDict)
        {
            Console.WriteLine($"Ключ: {pair.Key}, Значение: {pair.Value}");
        }

        Console.WriteLine();

        Console.WriteLine("Тестирование DoublyLinkedList");
        var myLinkedList = new DoublyLinkedList<int>();
        myLinkedList.Add(100);
        myLinkedList.Add(200);
        myLinkedList.Add(300);
        Console.WriteLine($"В списке элементов: {myLinkedList.Count}");
        
        foreach (var element in myLinkedList)
        {
            Console.WriteLine($"Элемент: {element}");
        }
    }
}
