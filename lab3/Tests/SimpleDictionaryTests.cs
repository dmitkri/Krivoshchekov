using lab3.Collections;
using Xunit;

namespace lab3.Tests;

public class SimpleDictionaryTests
{
    [Fact]
    public void Add_ShouldIncreaseCount()
    {
        // Создаем словарь
        var dict = new SimpleDictionary<string, int>();
        
        // Добавляем элементы
        dict.Add("один", 1);
        dict.Add("два", 2);
        
        // Проверяем количество
        Assert.Equal(2, dict.Count);
    }

    [Fact]
    public void Indexer_ShouldGetAndSet()
    {
        var dict = new SimpleDictionary<string, int>();
        dict.Add("один", 1);
        
        // Проверяем получение
        Assert.Equal(1, dict["один"]);
        
        // Проверяем установку
        dict["один"] = 100;
        Assert.Equal(100, dict["один"]);
    }

    [Fact]
    public void ContainsKey_ShouldReturnTrueForExistingKey()
    {
        var dict = new SimpleDictionary<string, int>();
        dict.Add("один", 1);
        
        // Проверяем наличие ключей
        Assert.True(dict.ContainsKey("один"));
        Assert.False(dict.ContainsKey("два"));
    }

    [Fact]
    public void Remove_ShouldDecreaseCount()
    {
        var dict = new SimpleDictionary<string, int>();
        dict.Add("один", 1);
        dict.Add("два", 2);
        
        // Удаляем элемент
        bool result = dict.Remove("один");
        
        // Проверяем результат
        Assert.True(result);
        Assert.Equal(1, dict.Count);
        Assert.False(dict.ContainsKey("один"));
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueForExistingKey()
    {
        var dict = new SimpleDictionary<string, int>();
        dict.Add("один", 1);
        
        // Пытаемся получить значение
        bool found = dict.TryGetValue("один", out int value);
        
        // Проверяем результат
        Assert.True(found);
        Assert.Equal(1, value);
        
        // Проверяем несуществующий ключ
        bool notFound = dict.TryGetValue("два", out _);
        Assert.False(notFound);
    }

    [Fact]
    public void GetEnumerator_ShouldIterateAllItems()
    {
        var dict = new SimpleDictionary<string, int>();
        dict.Add("один", 1);
        dict.Add("два", 2);
        
        // Собираем все элементы через foreach
        var collected = new List<KeyValuePair<string, int>>();
        foreach (var pair in dict)
        {
            collected.Add(pair);
        }
        
        // Проверяем результат
        Assert.Equal(2, collected.Count);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var dict = new SimpleDictionary<string, int>();
        dict.Add("один", 1);
        dict.Add("два", 2);
        
        // Очищаем словарь
        dict.Clear();
        
        // Проверяем, что словарь пустой
        Assert.Equal(0, dict.Count);
    }
}
