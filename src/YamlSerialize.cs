using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Core.Events;

namespace physgui
{
    public class YamlSerialize
    {
        static public void serialize(PhysicsSystem data, TextWriter output)
        {
            var yaml = new YamlStream(
                        new YamlDocument(new YamlMappingNode())
                    );
            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            root.Add("phys", showPhys(data));
            yaml.Save(output, assignAnchors: false);
        }

        static private YamlSequenceNode showVector(Vector vec)
        {
            var seq = new YamlSequenceNode {
                new YamlScalarNode(vec.X.ToString()),
                new YamlScalarNode(vec.Y.ToString())
            };
            seq.Style = SequenceStyle.Flow;
            return seq;
        }

        static private YamlMappingNode showObject(PhysicalObject obj)
        {
            var node = new YamlMappingNode { };
            node.Add("pos", showVector(obj.Position));
            if (obj.Movement.Length > 0)
            {
                node.Add("mov", showVector(obj.Movement));
            }
            node.Add("mass", obj.Mass.ToString());
            node.Add("radius", obj.Radius.ToString());
            return node;
        }


        static private YamlMappingNode showPhys(PhysicsSystem phys)
        {
            var node = new YamlMappingNode { };
            if (phys.Density > 0)
            {
                node.Add("density", phys.Density.ToString());
            }
            if (phys.IsGravityEnabled)
            {
                node.Add("gravity", phys.IsGravityEnabled.ToString().ToLower());
            }
            if (phys.AccelerationOfGravity.Length > 0)
            {
                node.Add("acceleration", showVector(phys.AccelerationOfGravity));
            }
            if (phys.Objects.Count > 0)
            {
                var seq = new YamlSequenceNode { };
                foreach (var obj in phys.Objects)
                {
                    seq.Add(showObject(obj));
                }
                node.Add("objects", seq);
            }
            return node;
        }
    }
}