using System.Collections;

namespace VenusRootLoader.Api;

public sealed class LocalizedData<T> : IReadOnlyDictionary<int, T>
{
    private Dictionary<int, T> UnderlyingDictionary { get; } = new();
    public IEnumerator<KeyValuePair<int, T>> GetEnumerator() => UnderlyingDictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => UnderlyingDictionary.Count;

    public T this[int key]
    {
        get
        {
            if (Count == 0)
            {
                T languageData = default!;
                UnderlyingDictionary.Add(key, languageData);
                return languageData!;
            }

            if (TryGetValue(key, out T value))
                return value;

            int firstLanguage = Keys.Min();
            return this[firstLanguage];
        }
        set => UnderlyingDictionary[key] = value;
    }

    public IEnumerable<int> Keys => UnderlyingDictionary.Keys;
    public IEnumerable<T> Values => UnderlyingDictionary.Values;
    public bool ContainsKey(int key) => UnderlyingDictionary.ContainsKey(key);
    public bool TryGetValue(int key, out T value) => UnderlyingDictionary.TryGetValue(key, out value);
}