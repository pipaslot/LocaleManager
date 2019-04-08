using System.Collections.Generic;

namespace LocaleManager.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrdefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default)
        {
            if (dic.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
