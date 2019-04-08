using LocaleManager.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
            foreach (var pair in content)
            {
                token.SetValue(pair.Key, pair.Value);
            }
            return JsonConvert.SerializeObject(token, Formatting.Indented);
        }
    }
}
