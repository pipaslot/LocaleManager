using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LocaleManager.Core.Extensions;

namespace LocaleManager.Core
{
    public class TranslationCollection
    {
        private readonly bool _filterPartialKeys;
        private readonly Dictionary<string, Dictionary<string, string>> _collection = new Dictionary<string, Dictionary<string, string>>();

        public ReadOnlyCollection<string> Locales { get; }
        public ReadOnlyCollection<string> Keys => new ReadOnlyCollection<string>(_collection.Select(c => c.Key).ToList());
        public TranslationCollection(bool filterPartialKeys) : this(new List<string>(), filterPartialKeys)
        {
        }

        public TranslationCollection(List<string> locales, bool filterPartialKeys)
        {
            _filterPartialKeys = filterPartialKeys;
            Locales = new ReadOnlyCollection<string>(locales.Distinct().ToList());
        }
        
        public void Set(Dictionary<string, string> source, string locale)
        {
            foreach (var s in source)
            {
                Set(s.Key, locale, s.Value);
            }
        }

        public void Set(string key, string locale, string value)
        {
            if (_collection.TryGetValue(key, out var translations))
            {
                translations[locale] = value;
            }
            else
            {
                var newTranslations = CreateTranslations();
                newTranslations[locale] = value;
                _collection[key] = newTranslations;
            }
        }

        public string Get(string key, string locale)
        {
            if (_collection.TryGetValue(key, out var translations))
            {
                return translations[locale];
            }
            return "";
        }

        public void Remove(string key)
        {
            _collection.Remove(key);
        }

        public Dictionary<string, string> GetAllTranslations(string locale)
        {
            var result = new Dictionary<string, string>();
            var ignored = GetPartialNodes();
            foreach (var translations in _collection)
            {
                if (!ignored.Contains(translations.Key))
                {
                    result.Add(translations.Key, translations.Value.GetOrdefault(locale, ""));
                }
            }
            return result;
        }

        private Dictionary<string, string> CreateTranslations(string defaultValue = "")
        {
            var result = new Dictionary<string, string>();
            foreach (var locale in Locales)
            {
                result.Add(locale, defaultValue);
            }
            return result;
        }

        public List<string> GetLocalesByCountOfValues()
        {
            var stats = new Dictionary<string, int>();
            foreach (var locale in Locales)
            {
                var count = _collection.Count(c => c.Value.Any(l => l.Key == locale && !string.IsNullOrWhiteSpace(l.Value)));
                stats.Add(locale, count);
            }
            return stats
                .OrderByDescending(c=>c.Value)
                .Select(c => c.Key)
                .ToList();
        }

        /// <summary>
        /// Returns nodes which are partially already in dictionary
        /// </summary>
        /// <returns></returns>
        public List<string> GetPartialNodes()
        {
            var result = new List<string>();
            if (!_filterPartialKeys)
            {
                return result;
            }
            foreach (var prop in _collection)
            {
                if (!IsPartial(prop.Key))
                {
                    result.Add(prop.Key);
                }

            }
            return result;
        }

        private bool IsPartial(string key)
        {
            var path = key.ToLower() + ".";
            return !_collection.Any(t => t.Key.ToLower().StartsWith(path));
        }
    }
}
