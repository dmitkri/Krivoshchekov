namespace lab3.Collections;

internal struct DictionaryEntry<TKey, TValue>
{
    public int Hash;
    public int NextIndex;
    public TKey Key;
    public TValue Value;
}

