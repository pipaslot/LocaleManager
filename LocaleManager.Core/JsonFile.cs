using System.IO;

namespace LocaleManager.Core
{
    public class JsonFile
    {
        public string Path { get; }
        public string Name { get; }
        public string Content { get; private set; }

        internal JsonFile(string path, string name = null)
        {
            Path = path;
            Name = !string.IsNullOrWhiteSpace(name) ? name : System.IO.Path.GetFileNameWithoutExtension(path);
            Content = File.ReadAllText(path);
        }

        public void Save(string content)
        {
            Content = content;
            File.WriteAllText(Path, content);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
