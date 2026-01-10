using System.Collections;

namespace lab3.Collections;

public class DoublyLinkedList<T> : IList<T>
{
    private LinkedListNode<T>? first;
    private LinkedListNode<T>? last;
    private int count;

    public DoublyLinkedList()
    {
        first = null;
        last = null;
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
            return FindNodeByIndex(index)!.Data;
        }
        set
        {
            if (index < 0 || index >= count)
            {
                throw new Exception("неправильный индекс");
            }
            FindNodeByIndex(index)!.Data = value;
        }
    }

    public void Add(T item)
    {
        LinkedListNode<T> newNode = new LinkedListNode<T>(item);

        if (first == null)
        {
            first = newNode;
            last = newNode;
        }
        else
        {
            last!.Next = newNode;
            newNode.Prev = last;
            last = newNode;
        }

        count = count + 1;
    }

    public void Clear()
    {
        first = null;
        last = null;
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
        if (arrayIndex < 0 || array.Length - arrayIndex < count)
        {
            throw new Exception("неправильный индекс или массив слишком маленький");
        }

        LinkedListNode<T>? current = first;
        int pos = arrayIndex;
        while (current != null)
        {
            array[pos] = current.Data;
            current = current.Next;
            pos = pos + 1;
        }
    }

    public int IndexOf(T item)
    {
        LinkedListNode<T>? current = first;
        int index = 0;

        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, item))
            {
                return index;
            }
            current = current.Next;
            index = index + 1;
        }

        return -1;
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > count)
        {
            throw new Exception("неправильный индекс");
        }

        LinkedListNode<T> newNode = new LinkedListNode<T>(item);

        if (index == 0)
        {
            if (first == null)
            {
                first = newNode;
                last = newNode;
            }
            else
            {
                newNode.Next = first;
                first.Prev = newNode;
                first = newNode;
            }
        }
        else if (index == count)
        {
            last!.Next = newNode;
            newNode.Prev = last;
            last = newNode;
        }
        else
        {
            LinkedListNode<T> nodeAt = FindNodeByIndex(index)!;
            newNode.Next = nodeAt;
            newNode.Prev = nodeAt.Prev;
            if (nodeAt.Prev != null)
            {
                nodeAt.Prev.Next = newNode;
            }
            nodeAt.Prev = newNode;
        }

        count = count + 1;
    }

    public bool Remove(T item)
    {
        LinkedListNode<T>? current = first;
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, item))
            {
                DeleteNode(current);
                return true;
            }
            current = current.Next;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= count)
        {
            throw new Exception("неправильный индекс");
        }

        DeleteNode(FindNodeByIndex(index)!);
    }

    public IEnumerator<T> GetEnumerator()
    {
        LinkedListNode<T>? current = first;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private LinkedListNode<T>? FindNodeByIndex(int index)
    {
        if (index < 0 || index >= count)
        {
            return null;
        }

        if (index < count / 2)
        {
            LinkedListNode<T>? current = first;
            for (int i = 0; i < index; i++)
            {
                current = current!.Next;
            }
            return current;
        }
        else
        {
            LinkedListNode<T>? current = last;
            for (int i = count - 1; i > index; i--)
            {
                current = current!.Prev;
            }
            return current;
        }
    }

    private void DeleteNode(LinkedListNode<T> node)
    {
        if (node.Prev != null)
        {
            node.Prev.Next = node.Next;
        }
        else
        {
            first = node.Next;
        }

        if (node.Next != null)
        {
            node.Next.Prev = node.Prev;
        }
        else
        {
            last = node.Prev;
        }

        count = count - 1;
    }
}
