using lab3.Collections;
using Xunit;

namespace lab3.Tests;

public class SimpleListTests
{
    [Fact]
    public void Add_ShouldIncreaseCount()
    {
        // Создаем список
        var list = new SimpleList<int>();
        
        // Добавляем элементы
        list.Add(5);
        list.Add(10);
        
        // Проверяем количество
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Indexer_ShouldGetAndSet()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        
        // Проверяем получение
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        
        // Проверяем установку
        list[0] = 100;
        Assert.Equal(100, list[0]);
    }

    [Fact]
    public void Remove_ShouldDecreaseCount()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        // Удаляем элемент
        bool result = list.Remove(2);
        
        // Проверяем результат
        Assert.True(result);
        Assert.Equal(2, list.Count);
        Assert.False(list.Contains(2));
    }

    [Fact]
    public void Contains_ShouldReturnTrueForExistingItem()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        
        // Проверяем наличие элементов
        Assert.True(list.Contains(1));
        Assert.False(list.Contains(99));
    }

    [Fact]
    public void IndexOf_ShouldReturnCorrectIndex()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        // Проверяем индексы
        Assert.Equal(1, list.IndexOf(2));
        Assert.Equal(-1, list.IndexOf(999));
    }

    [Fact]
    public void Insert_ShouldAddAtSpecifiedIndex()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(3);
        
        // Вставляем элемент
        list.Insert(1, 2);
        
        // Проверяем результат
        Assert.Equal(3, list.Count);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        
        // Очищаем список
        list.Clear();
        
        // Проверяем, что список пустой
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void GetEnumerator_ShouldIterateAllItems()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        // Собираем все элементы через foreach
        var collected = new List<int>();
        foreach (var item in list)
        {
            collected.Add(item);
        }
        
        // Проверяем результат
        Assert.Equal(3, collected.Count);
        Assert.Equal(1, collected[0]);
        Assert.Equal(2, collected[1]);
        Assert.Equal(3, collected[2]);
    }
}
