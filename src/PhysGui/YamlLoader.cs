using YamlDotNet.Serialization;

namespace PhysGui
{

    public class PhysicsConfigParser
    {

        private class PhysicsObjectRep
        {
            [YamlMember(Alias = "pos")]
            public double[] Pos { get; set; } = null!; // required

            [YamlMember(Alias = "mov")]
            public double[] Mov { get; set; } = new double[] { 0, 0 };

            [YamlMember(Alias = "mass")]
            public double Mass { get; set; } // required

            [YamlMember(Alias = "radius")]
            public double Radius { get; set; } // required
        }

        private class PhysicsConfigRep
        {
            [YamlMember(Alias = "density")]
            public double Density { get; set; } = 0;

            [YamlMember(Alias = "accel")]
            public double[] Accel { get; set; } = new double[] { 0, 0 };

            [YamlMember(Alias = "wind")]
            public double[] Wind { get; set; } = new double[] { 0, 0 };

            [YamlMember(Alias = "gravity")]
            public bool Gravity { get; set; } = false;

            [YamlMember(Alias = "objs")]
            public List<PhysicsObjectRep> Objs { get; set; } = new List<PhysicsObjectRep>();
        }

        private class Root
        {
            [YamlMember(Alias = "phys")]
            public PhysicsConfigRep? Phys { get; set; } = null;
        }

        private readonly IDeserializer _deserializer;
        private Root? root;

        public PhysicsConfigParser(string filePath)
        {
            _deserializer = new DeserializerBuilder().Build();
            ParseFile(filePath);
        }

        private Root Parse(string yamlContent)
        {
            var root = _deserializer.Deserialize<Root>(yamlContent);
            ValidateConfig(root);
            return root;
        }

        private void ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            string yamlContent = File.ReadAllText(filePath);
            root = Parse(yamlContent);
        }

        private void ValidateConfig(Root root)
        {
            if (root.Phys == null)
                throw new InvalidDataException("Required phys section");

            var config = root.Phys;
            
            if (config.Objs == null || config.Objs.Count == 0)
                throw new InvalidDataException("Objects list (objs) cannot be empty");

            if (config.Density < 0)
                throw new InvalidDataException($"Field 'density' must be a non-negative number");


            for (int i = 0; i < config.Objs.Count; i++)
            {
                var obj = config.Objs[i];

                if (obj.Pos == null || obj.Pos.Length != 2)
                    throw new InvalidDataException($"Object {i}: field 'pos' is required and must contain 2 values");

                if (obj.Mass <= 0)
                    throw new InvalidDataException($"Object {i}: field 'mass' must be a non-negative number");

                if (obj.Radius <= 0)
                    throw new InvalidDataException($"Object {i}: field 'radius' must be a non-negative number");

                if (obj.Mov == null)
                {
                    obj.Mov = new double[] { 0, 0 };
                }
                else if (obj.Mov.Length != 2)
                {
                    throw new InvalidDataException($"Object {i}: field 'mov' is required and must contain 2 values");
                }
            }

            if (config.Accel == null)
            {
                config.Accel = new double[] { 0, 0 };
            }
            else if (config.Accel.Length != 2)
            {
                throw new InvalidDataException($"Field 'accel' is required and must contain 2 values");
            }

            if (config.Wind == null)
            {
                config.Wind = new double[] { 0, 0 };
            }
            else if (config.Wind.Length != 2)
            {
                throw new InvalidDataException($"Field 'wind' is required and must contain 2 values");
            }
        }

        public PhysicsSystem createPhysicsSystem()
        {
            var rep = root!.Phys!;
            var phys = new PhysicsSystem(rep.Objs.Count);
            phys.Density = rep.Density;
            phys.AccelerationOfGravity.X = rep.Accel[0];
            phys.AccelerationOfGravity.Y = rep.Accel[1];
            phys.Wind.X = rep.Wind[0];
            phys.Wind.Y = rep.Wind[1];
            phys.IsGravityEnabled = rep.Gravity;
            for (int i = 0; i < rep.Objs.Count; i++)
            {
                var pobj = phys.Objects[i];
                var robj = rep.Objs[i];
                pobj.Mass = robj.Mass;
                pobj.Radius = robj.Radius;
                pobj.Position.X = robj.Pos[0];
                pobj.Position.Y = robj.Pos[1];
                pobj.Movement.X = robj.Mov[0];
                pobj.Movement.Y = robj.Mov[1];
            }
            return phys;
        }
    }


}
