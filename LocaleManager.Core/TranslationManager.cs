﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LocaleManager.Core.Serialization;

namespace LocaleManager.Core
{
    public class TranslationManager
    {
        public TranslationCollection Translations { get; private set; } = new TranslationCollection(false);
        public ReadOnlyCollection<string> Locales => Translations.Locales;

        private readonly List<JsonFile> _files = new List<JsonFile>();

        private ISerializer _serializer;

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

            _serializer = DetectSerializer(_files);
            if (_serializer == null)
            {
                return;
            }
            var locales = _files.Select(f => f.Name).ToList();
            Translations = new TranslationCollection(locales, !_serializer.IsPartialKeySupported);
            foreach (var file in _files)
            {
                var translations = _serializer.Deserialize(file.Content);
                Translations.Set(translations, file.Name);
            }
        }

        public bool Save()
        {
            if (_serializer == null) return false;
            foreach (var file in _files)
            {
                var translations = Translations.GetAllTranslations(file.Name);
                var content = _serializer.Serialize(translations);
                file.Save(content);
            }

            return true;
        }

        private ISerializer DetectSerializer(ICollection<JsonFile> files)
        {
            foreach (var file in files)
            {
                if (file == null) continue;
                if (file.IsEmpty()) continue;

                var treeSerializer = new JsonTreeSerializer();
                var treeContent = treeSerializer.Serialize(treeSerializer.Deserialize(file.Content));
                var treeDiff = GetDiff(treeContent, file.Content);


                var dicSerializer = new JsonDictionarySerializer();
                var dicContent = dicSerializer.Serialize(dicSerializer.Deserialize(file.Content));
                var dicDiff = GetDiff(dicContent, file.Content);
                return treeDiff < dicDiff
                    ? (ISerializer)treeSerializer
                    : dicSerializer;

            }
            return null;
        }

        private int GetDiff(string first, string second)
        {
            var left = RemoveWhitespaces(first);
            var right = RemoveWhitespaces(second);
            return Math.Abs(left.Length - right.Length);
        }

        private string RemoveWhitespaces(string value)
        {
            return Regex.Replace(value, @"\s+", "");
        }
    }
}
