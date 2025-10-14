using YamlDotNet.RepresentationModel;

namespace PhysGui
{
    public class YamlSerialize
    {
        private YamlStream yaml;
        private YamlMappingNode root;
        private string path;



        public YamlSerialize(string path)
        {
            this.path = path;
            yaml = new YamlStream();
            using (var reader = new StreamReader(path))
                yaml.Load(reader);
            root = (YamlMappingNode)yaml.Documents[0].RootNode;
        }

        public PhysicsSystem createSystem()
        {
            var physNode = GetValue<YamlMappingNode>(root, "phys");

            return null;
        }

        private static T GetValue<T>(this YamlMappingNode map, string key)
        {
            var keyNode = new YamlScalarNode(key);

            if (!map.Children.ContainsKey(keyNode))
                throw new ConfigException($"Key '{key}' not found in YAML mapping.");

            if (map.Children[keyNode] is T value)
                return value;

            throw new ConfigException($"Value of '{key}' is not a {typeof(T).Name} node.");
        }

        public void save()
        {
            using (var writer = new StreamWriter(path))
                yaml.Save(writer, assignAnchors: false);
        }
    }

    public class ConfigException : Exception
    {
        public ConfigException(string error)
            : base(error) { }
    }

}
