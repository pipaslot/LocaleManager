using System.Collections.Generic;

namespace LocaleManager.Core.Serialization
{
    public interface ISerializer
    {
        Dictionary<string, string> Deserialize(string content);
        string Serialize(Dictionary<string, string> content);
        bool IsPartialKeySupported { get; }
    }
}