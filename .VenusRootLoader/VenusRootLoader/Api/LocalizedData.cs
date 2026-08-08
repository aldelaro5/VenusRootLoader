using System.Collections;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api;

public sealed class LocalizedData<T> : IReadOnlyDictionary<Branch<LanguageLeaf>, T>
{
    private Dictionary<Branch<LanguageLeaf>, T> UnderlyingDictionary { get; } = new();
    public IEnumerator<KeyValuePair<Branch<LanguageLeaf>, T>> GetEnumerator() => UnderlyingDictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => UnderlyingDictionary.Count;

    public T this[Branch<LanguageLeaf> language]
    {
        get
        {
            if (Count == 0)
            {
                T languageData = default!;
                UnderlyingDictionary.Add(language, languageData);
                return languageData;
            }

            if (TryGetValue(language, out T value))
                return value;

            int minGameId = Keys.Min(x => x.Resolve().GameId);
            Branch<LanguageLeaf> firstLanguage = RegistryResolver.Resolve<LanguageLeaf>().GetByGameId(minGameId);
            return this[firstLanguage];
        }
        set => UnderlyingDictionary[language] = value;
    }

    public IEnumerable<Branch<LanguageLeaf>> Keys => UnderlyingDictionary.Keys;
    public IEnumerable<T> Values => UnderlyingDictionary.Values;
    public bool ContainsKey(Branch<LanguageLeaf> key) => UnderlyingDictionary.ContainsKey(key);
    public bool TryGetValue(Branch<LanguageLeaf> key, out T value) => UnderlyingDictionary.TryGetValue(key, out value);
}