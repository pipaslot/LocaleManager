using System.Collections.Generic;
using System.Linq;
using LocaleManager.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocaleManager.Core.Serialization
{
    public class JsonTreeSerializer : ISerializer
    {
        public Dictionary<string, string> Deserialize(string content)
        {
            var tree = JObject.Parse(content);
            return tree.ToDictionary();
        }

        public string Serialize(Dictionary<string, string> content)
        {
            var token = new JObject();
            var keys = content.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
            {
                var value = content[key];
                if (!string.IsNullOrEmpty(value))
                {
                    token.SetValue(key, value);
                }
            }
            return JsonConvert.SerializeObject(token, Formatting.Indented);
        }
        
        public bool IsPartialKeySupported => false;
    }
}
