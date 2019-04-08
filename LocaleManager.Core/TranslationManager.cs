using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LocaleManager.Core
{
    public class TranslationManager
    {
        public TranslationCollection Translations { get; private set; } = new TranslationCollection();
        public ReadOnlyCollection<string> Locales => Translations.Locales;
        private List<JsonFile> _files { get; } = new List<JsonFile>();
        private JsonSerializer _serializer = new JsonSerializer();

        public void LoadFiles(string path, string pattern = "*.json")
        {
            _files.Clear();
            var names = Directory.EnumerateFiles(path);
            var regex = FilePattern.CreateRegex(pattern);
            foreach (string s in names)
            {
                if (regex.IsMatch(s))
                {
                    var file = new JsonFile(s);
                    _files.Add(file);
                }
            }
            var locales = _files.Select(f => f.Name).ToList();
            Translations = new TranslationCollection(locales);
            foreach (var file in _files)
            {
                var translations = _serializer.Deserialize(file.Content);
                Translations.Set(translations, file.Name);
            }
        }

        public void Save()
        {
            foreach (var file in _files)
            {
                var translations = Translations.GetAllTranslations(file.Name);
                var content = _serializer.Serialize(translations);
                file.Save(content);
            }
        }
    }
}
