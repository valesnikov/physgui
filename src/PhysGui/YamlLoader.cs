using YamlDotNet.RepresentationModel;

namespace PhysGui
{
    public class YamlLoader
    {
        private YamlStream yaml = new YamlStream();
        private string path;

        public YamlLoader(string path)
        {
            this.path = path;
            using (var reader = new StreamReader(path))
                yaml.Load(reader);
        }

        public YamlNode GetRoot()
        {
            return yaml.Documents[0].RootNode;
        }

        public void Save()
        {
            using (var writer = new StreamWriter(path))
                yaml.Save(writer, assignAnchors: false);
        }
    }

}
