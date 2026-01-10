using System.Collections;

namespace lab3.Collections;

public class SimpleList<T> : IList<T>, ICollection<T>, IEnumerable<T>
{
    private T[] items;
    private int count;
    private const int StartSize = 4;

    public SimpleList()
    {
        items = new T[StartSize];
        count = 0;
    }

    public SimpleList(int capacity)
    {
        if (capacity < 0)
        {
            throw new Exception("размер не может быть отрицательным");
        }
        items = new T[capacity];
        count = 0;
    }

    public int Count => count;

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
            {
                throw new Exception("неправильный индекс");
            }
            return items[index];
        }
        set
        {
            if (index < 0 || index >= count)
            {
                throw new Exception("неправильный индекс");
            }
            items[index] = value;
        }
    }

    public void Add(T item)
    {
        if (count >= items.Length)
        {
            IncreaseSize();
        }
        items[count] = item;
        count = count + 1;
    }

    public void Clear()
    {
        for (int i = 0; i < count; i++)
        {
            items[i] = default(T)!;
        }
        count = 0;
    }

    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new Exception("массив не может быть пустым");
        }
        if (arrayIndex < 0)
        {
            throw new Exception("индекс должен быть положительным");
        }
        if (array.Length - arrayIndex < count)
        {
            throw new Exception("массив слишком маленький");
        }

        for (int i = 0; i < count; i++)
        {
            array[arrayIndex + i] = items[i];
        }
    }

    public int IndexOf(T item)
    {
        for (int i = 0; i < count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(items[i], item))
            {
                return i;
            }
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > count)
        {
            throw new Exception("неправильный индекс");
        }

        if (count >= items.Length)
        {
            IncreaseSize();
        }

        for (int i = count; i > index; i--)
        {
            items[i] = items[i - 1];
        }

        items[index] = item;
        count = count + 1;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= count)
        {
            throw new Exception("неправильный индекс");
        }

        for (int i = index; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }

        items[count - 1] = default(T)!;
        count = count - 1;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
        {
            yield return items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void IncreaseSize()
    {
        int newSize = items.Length == 0 ? StartSize : items.Length * 2;
        T[] newItems = new T[newSize];

        for (int i = 0; i < count; i++)
        {
            newItems[i] = items[i];
        }

        items = newItems;
    }
}
