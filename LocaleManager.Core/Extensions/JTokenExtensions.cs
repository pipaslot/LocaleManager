using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocaleManager.Core.Extensions
{
    public static class JTokenExtensions
    {
        public static Dictionary<string, string> ToDictionary(this JToken token)
        {
            var result = new Dictionary<string, string>();
            token.AggregateData(result);
            return result;
        }

        public static void AggregateData(this JToken token, Dictionary<string, string> data)
        {
            if (token is JValue value)
            {
                if (!data.ContainsKey(token.Path))
                {
                    data.Add(token.Path, value.Value?.ToString());
                }
            }
            else
            {
                foreach (var child in token)
                {
                    child.AggregateData(data);
                }
            }
        }

        /// <summary>
        /// Set node value. Returns true if node was created
        /// </summary>
        public static bool SetValue(this JToken token, string path, string defaultValue = "")
        {
            var parts = path.Split('.');
            return token.SetValue(parts, defaultValue);
        }

        /// <summary>
        /// Set node value. Returns true if node was created
        /// </summary>
        public static bool SetValue(this JToken token, string[] path, string defaultValue = "")
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if (path == null || path.Length == 0)
            {
                return false;
            }
            var first = path.First();
            var remaining = path.Skip(1).ToArray();
            if (token is JValue value)
            {
                var newNode = new JObject();
                token.Replace(newNode);
                if (!string.IsNullOrWhiteSpace(value.Value.ToString()))
                {
                    newNode["__"] = token;
                }

                return newNode.SetValue(remaining, defaultValue);
            }
            if (token[first] == null)
            {
                if (remaining.Length > 0)
                {
                    token[first] = new JObject();
                    return token[first].SetValue(remaining, defaultValue);
                }
                else
                {
                    token[first] = defaultValue;
                    return true;
                }                
            }
            return token[first].SetValue(remaining, defaultValue);
        }
    }
}
