using System.Collections;

namespace lab3.Collections;

public class SimpleDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private int[] buckets;
    private DictionaryEntry<TKey, TValue>[] entries;
    private int count;
    private int freeIndex;
    private int freeCount;
    private const int InitialSize = 4;

    public SimpleDictionary()
    {
        buckets = new int[InitialSize];
        entries = new DictionaryEntry<TKey, TValue>[InitialSize];
        count = 0;
        freeIndex = -1;
        freeCount = 0;

        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = -1;
        }
    }

    public int Count => count - freeCount;

    public bool IsReadOnly => false;

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue? value))
            {
                return value;
            }
            throw new Exception($"ключ '{key}' отсутствует");
        }
        set
        {
            AddOrUpdate(key, value, false);
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> keysList = new List<TKey>();
            for (int i = 0; i < count; i++)
            {
                if (entries[i].Hash >= 0)
                {
                    keysList.Add(entries[i].Key);
                }
            }
            return keysList;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> valuesList = new List<TValue>();
            for (int i = 0; i < count; i++)
            {
                if (entries[i].Hash >= 0)
                {
                    valuesList.Add(entries[i].Value);
                }
            }
            return valuesList;
        }
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public void Add(TKey key, TValue value)
    {
        if (!AddOrUpdate(key, value, true))
        {
            throw new Exception($"ключ '{key}' уже существует");
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        if (count > 0)
        {
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = -1;
            }
            for (int i = 0; i < count; i++)
            {
                entries[i] = default(DictionaryEntry<TKey, TValue>);
            }
            freeIndex = -1;
            count = 0;
            freeCount = 0;
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (TryGetValue(item.Key, out TValue? value))
        {
            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null || arrayIndex < 0 || array.Length - arrayIndex < Count)
        {
            throw new Exception("неправильные параметры");
        }

        int pos = arrayIndex;
        foreach (var entry in this)
        {
            array[pos] = entry;
            pos = pos + 1;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
        {
            if (entries[i].Hash >= 0)
            {
                yield return new KeyValuePair<TKey, TValue>(entries[i].Key, entries[i].Value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        if (key == null)
        {
            throw new Exception("ключ не может быть пустым");
        }

        int hashCode = GetHashCode(key);
        int bucketIndex = hashCode % buckets.Length;

        int previousIndex = -1;
        int currentIndex = buckets[bucketIndex];

        while (currentIndex >= 0)
        {
            if (entries[currentIndex].Hash == hashCode && 
                EqualityComparer<TKey>.Default.Equals(entries[currentIndex].Key, key))
            {
                if (previousIndex < 0)
                {
                    buckets[bucketIndex] = entries[currentIndex].NextIndex;
                }
                else
                {
                    entries[previousIndex].NextIndex = entries[currentIndex].NextIndex;
                }

                entries[currentIndex].Hash = -1;
                entries[currentIndex].NextIndex = freeIndex;
                entries[currentIndex].Key = default(TKey)!;
                entries[currentIndex].Value = default(TValue)!;
                freeIndex = currentIndex;
                freeCount = freeCount + 1;

                return true;
            }

            previousIndex = currentIndex;
            currentIndex = entries[currentIndex].NextIndex;
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Contains(item) && Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        {
            throw new Exception("ключ не может быть пустым");
        }

        int hashCode = GetHashCode(key);
        int bucketIndex = hashCode % buckets.Length;

        int currentIndex = buckets[bucketIndex];
        while (currentIndex >= 0)
        {
            if (entries[currentIndex].Hash == hashCode && 
                EqualityComparer<TKey>.Default.Equals(entries[currentIndex].Key, key))
            {
                value = entries[currentIndex].Value;
                return true;
            }
            currentIndex = entries[currentIndex].NextIndex;
        }

        value = default(TValue)!;
        return false;
    }

    private int GetHashCode(TKey key)
    {
        int hashCode = key.GetHashCode();
        return hashCode < 0 ? -hashCode : hashCode;
    }

    private bool AddOrUpdate(TKey key, TValue value, bool onlyAdd)
    {
        if (key == null)
        {
            throw new Exception("ключ не может быть пустым");
        }

        int hashCode = GetHashCode(key);
        int bucketIndex = hashCode % buckets.Length;

        int currentIndex = buckets[bucketIndex];
        while (currentIndex >= 0)
        {
            if (entries[currentIndex].Hash == hashCode && 
                EqualityComparer<TKey>.Default.Equals(entries[currentIndex].Key, key))
            {
                if (onlyAdd)
                {
                    return false;
                }
                entries[currentIndex].Value = value;
                return true;
            }
            currentIndex = entries[currentIndex].NextIndex;
        }

        int newIndex;
        if (freeCount > 0)
        {
            newIndex = freeIndex;
            freeIndex = entries[newIndex].NextIndex;
            freeCount = freeCount - 1;
        }
        else
        {
            if (count == entries.Length)
            {
                Resize();
                bucketIndex = hashCode % buckets.Length;
            }
            newIndex = count;
            count = count + 1;
        }

        entries[newIndex].Hash = hashCode;
        entries[newIndex].NextIndex = buckets[bucketIndex];
        entries[newIndex].Key = key;
        entries[newIndex].Value = value;
        buckets[bucketIndex] = newIndex;

        return true;
    }

    private void Resize()
    {
        int newSize = buckets.Length * 2;
        int[] newBuckets = new int[newSize];
        DictionaryEntry<TKey, TValue>[] newEntries = new DictionaryEntry<TKey, TValue>[newSize];

        for (int i = 0; i < newBuckets.Length; i++)
        {
            newBuckets[i] = -1;
        }

        for (int i = 0; i < count; i++)
        {
            newEntries[i] = entries[i];
        }

        for (int i = 0; i < count; i++)
        {
            if (newEntries[i].Hash >= 0)
            {
                int newBucketIndex = newEntries[i].Hash % newSize;
                newEntries[i].NextIndex = newBuckets[newBucketIndex];
                newBuckets[newBucketIndex] = i;
            }
        }

        buckets = newBuckets;
        entries = newEntries;
    }
}
