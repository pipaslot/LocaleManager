using LocaleManager.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace LocaleManager.Core
{
    public class JsonSerializer
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
    }
}
