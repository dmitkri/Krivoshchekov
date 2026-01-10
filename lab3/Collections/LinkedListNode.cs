namespace lab3.Collections;

internal class LinkedListNode<T>
{
    public T Data;
    public LinkedListNode<T>? Prev;
    public LinkedListNode<T>? Next;

    public LinkedListNode(T data)
    {
        Data = data;
        Prev = null;
        Next = null;
    }
}

