using System.Collections.Generic;
using System.Linq;

namespace FFXIVMultiLang.Extensions;

public static class DictionaryExtensions
{
    //! https://www.codeproject.com/Tips/494499/Implementing-Dictionary-RemoveAll
    public static bool RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> match, bool dispose = false)
    {
        var anyRemoved = false;

        foreach (var key in dict.Keys.ToArray())
        {
            if (!dict.TryGetValue(key, out var value) || !match(key, value))
                continue;

            if (dispose && value is IDisposable disposable)
                disposable.Dispose();

            anyRemoved |= dict.Remove(key);
        }

        return anyRemoved;
    }

    public static void Dispose<K, V>(this IDictionary<K, V> dict)
    {
        dict.Values.OfType<IDisposable>().ForEach(disposable => disposable.Dispose());
        dict.Clear();
    }
}
