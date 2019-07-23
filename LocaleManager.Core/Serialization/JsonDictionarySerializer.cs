using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LocaleManager.Core.Serialization
{
    public class JsonDictionarySerializer : ISerializer
    {
        public Dictionary<string, string> Deserialize(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public string Serialize(Dictionary<string, string> content)
        {
            var list = content
                .Select(p => (p.Key, p.Value))
                .OrderBy(p =>p.Key)
                .Select(p => $"\t\"{p.Key}\": \"{p.Value?.Replace("\"", "\\\"")}\"")
                .ToList();
            var serialized = string.Join($",{Environment.NewLine}", list);

            return $@"{{{Environment.NewLine}{serialized}{Environment.NewLine}}}";
        }
        
        public bool IsPartialKeySupported => true;

        private List<(string Key, string Value)> ToList(Dictionary<string, string> content)
        {
            return content
                .Select(p => (p.Key, p.Value))
                .ToList();
        }

    }
}
