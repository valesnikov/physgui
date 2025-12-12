using YamlDotNet.Serialization;
using System.Drawing;


namespace PhysGui
{

    public class PhysicsConfigParser
    {

        private class Polygon
        {
            [YamlMember(Alias = "verts")]
            public float[][] Verts { get; set; } = null!; // required

            [YamlMember(Alias = "color")]
            public string Color { get; set; } = "#AFAFAF";
        }

        private class Rectangle
        {
            [YamlMember(Alias = "pos")]
            public float[] Pos { get; set; } = null!;

            [YamlMember(Alias = "w")]
            public float Width { get; set; } // required

            [YamlMember(Alias = "h")]
            public float Height { get; set; } // required

            [YamlMember(Alias = "color")]
            public string Color { get; set; } = "#AFAFAF";
        }

        private class Line
        {
            [YamlMember(Alias = "start")]
            public float[] Start { get; set; } = null!;

            [YamlMember(Alias = "end")]
            public float[] End { get; set; } = null!;

            [YamlMember(Alias = "thick")]
            public float Thickness { get; set; } = 0.03f;

            [YamlMember(Alias = "color")]
            public string Color { get; set; } = "#AFAFAF";
        }

        private class BackGround
        {
            [YamlMember(Alias = "polys")]
            public Polygon[] Polygons { get; set; } = new Polygon[] { };

            [YamlMember(Alias = "rects")]
            public Rectangle[] Rectangles { get; set; } = new Rectangle[] { };

            [YamlMember(Alias = "lines")]
            public Line[] Lines { get; set; } = new Line[] { };

            [YamlMember(Alias = "color")]
            public string Color { get; set; } = "#181818";
        }

        private class PhysicsObjectRep
        {
            [YamlMember(Alias = "pos")]
            public double[] Pos { get; set; } = null!; // required

            [YamlMember(Alias = "mov")]
            public double[] Mov { get; set; } = new double[] { 0, 0 };

            [YamlMember(Alias = "mass")]
            public double Mass { get; set; } // required

            [YamlMember(Alias = "bounce")]
            public double Bounce { get; set; } = 0;

            [YamlMember(Alias = "radius")]
            public double Radius { get; set; } // required

            [YamlMember(Alias = "color")]
            public string Color { get; set; } = "#ffffff";
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
            public List<PhysicsObjectRep> Objs { get; set; } = new();
        }

        private class CameraRep
        {
            [YamlMember(Alias = "center")]
            public double[] Center { get; set; } = new double[] { 0, 0 };

            [YamlMember(Alias = "scale")]
            public double Scale { get; set; } = 1;
        }

        private class Root
        {
            [YamlMember(Alias = "phys")]
            public PhysicsConfigRep Phys { get; set; } = new();

            [YamlMember(Alias = "cam")]
            public CameraRep CameraRep { get; set; } = new();

            [YamlMember(Alias = "bg")]
            public BackGround Back { get; set; } = new();
        }

        private readonly IDeserializer deserializer;
        private Root root;

        public PhysicsConfigParser(string filePath)
        {
            deserializer = new DeserializerBuilder().Build();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            string yamlContent = File.ReadAllText(filePath);
            root = deserializer.Deserialize<Root>(yamlContent);
            ValidateConfig(root);
        }

        private void ValidateConfig(Root root)
        {
            var physCfg = root.Phys;

            if (physCfg.Objs == null || physCfg.Objs.Count == 0)
                throw new InvalidDataException("Objects list (phys.objs) cannot be empty");

            if (physCfg.Density < 0)
                throw new InvalidDataException($"Field 'phys.density' must be a non-negative number");


            for (int i = 0; i < physCfg.Objs.Count; i++)
            {
                var obj = physCfg.Objs[i];

                if (obj.Pos == null || obj.Pos.Length != 2)
                    throw new InvalidDataException($"Object {i}: field 'phys.obj.pos' is required and must contain 2 values");

                if (obj.Mass == 0)
                    throw new InvalidDataException($"Object {i}: field 'phys.obj.mass' must be a non-zero number");

                if (obj.Bounce < 0 || obj.Bounce > 1)
                    throw new InvalidDataException($"Object {i}: field 'phys.obj.bounce' must be in [0, 1]");

                if (obj.Radius <= 0)
                    throw new InvalidDataException($"Object {i}: field 'phys.obj.radius' must be a non-negative number");

                if (obj.Mov == null)
                {
                    obj.Mov = new double[] { 0, 0 };
                }
                else if (obj.Mov.Length != 2)
                {
                    throw new InvalidDataException($"Object {i}: field 'phys.obj.mov' is required and must contain 2 values");
                }
            }

            if (physCfg.Accel == null)
            {
                physCfg.Accel = new double[] { 0, 0 };
            }
            else if (physCfg.Accel.Length != 2)
            {
                throw new InvalidDataException($"Field 'phys.accel' is required and must contain 2 values");
            }

            if (physCfg.Wind == null)
            {
                physCfg.Wind = new double[] { 0, 0 };
            }
            else if (physCfg.Wind.Length != 2)
            {
                throw new InvalidDataException($"Field 'phys.wind' is required and must contain 2 values");
            }

            var camCfg = root.CameraRep;

            if (camCfg.Center.Length != 2)
            {
                throw new InvalidDataException($"Field 'cam.center' must contain 2 values");
            }
            if (camCfg.Scale <= 0.0)
            {
                throw new InvalidDataException($"Field 'cam.scale' must be a positive number");
            }


            foreach (var poly in root.Back.Polygons)
            {
                if (poly.Verts.Length < 3)
                {
                    throw new InvalidDataException($"Field 'bg.poly.verts' must contain at least 3 vert");
                }
                foreach (var pos in poly.Verts)
                {
                    if (pos.Length != 2)
                    {
                        throw new InvalidDataException($"Field 'bg.poly.vert' must contain 2 values");
                    }
                }
            }

            foreach (var rect in root.Back.Rectangles)
            {
                if (rect.Pos.Length != 2)
                {
                    throw new InvalidDataException($"Field 'bg.rect.pos' must contain 2 values");
                }
                if (rect.Width <= 0)
                {
                    throw new InvalidDataException($"Field 'bg.rect.w' must be a positive number");
                }
                if (rect.Height <= 0)
                {
                    throw new InvalidDataException($"Field 'bg.rect.h' must be a positive number");
                }

            }

            foreach (var line in root.Back.Lines)
            {
                if (line.Start.Length != 2)
                {
                    throw new InvalidDataException($"Field 'bg.line.start' must contain 2 values");
                }
                if (line.End.Length != 2)
                {
                    throw new InvalidDataException($"Field 'bg.line.end' must contain 2 values");
                }
                if (line.Thickness <= 0)
                {
                    throw new InvalidDataException($"Field 'bg.line.thick' must be a positive number");
                }
            }
        }

        public ((double x, double y) center, double scale) getCameraPosition()
        {
            var x = root.CameraRep.Center[0];
            var y = root.CameraRep.Center[1];
            var scale = root.CameraRep.Scale;
            return ((x, y), scale);
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
                pobj.Bounce = robj.Bounce;

                Color color = ColorTranslator.FromHtml(robj.Color);
                pobj.Color = (color.R, color.G, color.B);
            }
            return phys;
        }

        private static (
            (float x, float y) p1,
            (float x, float y) p2,
            (float x, float y) p3,
            (float x, float y) p4
        ) GetLineRectangle(
            float x1, float y1,
            float x2, float y2,
            float thickness)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;

            float len = MathF.Sqrt(dx * dx + dy * dy);

            float px = -dy / len;
            float py = dx / len;

            float h = thickness * 0.5f;

            var p1 = (x1 + px * h, y1 + py * h);
            var p2 = (x1 - px * h, y1 - py * h);
            var p3 = (x2 - px * h, y2 - py * h);
            var p4 = (x2 + px * h, y2 + py * h);

            return (p1, p2, p3, p4);
        }


        public BackGlBuilder createBack()
        {
            var bb = new BackGlBuilder();

            bb.SetBgColor(ColorTranslator.FromHtml(root.Back.Color));

            foreach (var poly in root.Back.Polygons)
            {
                Color polyColor = ColorTranslator.FromHtml(poly.Color);
                var v0 = poly.Verts[0];
                for (int i = 1; i + 1 < poly.Verts.Length; i++)
                {
                    var v1 = poly.Verts[i];
                    var v2 = poly.Verts[i + 1];
                    bb.Add(new Triangle((v0[0], v0[1]), (v1[0], v1[1]), (v2[0], v2[1]), polyColor));
                }
            }
            foreach (var rect in root.Back.Rectangles)
            {
                Color rectColor = ColorTranslator.FromHtml(rect.Color);
                var v0 = (rect.Pos[0], rect.Pos[1]);
                var v1 = (rect.Pos[0] + rect.Width, rect.Pos[1]);
                var v2 = (rect.Pos[0] + rect.Width, rect.Pos[1] - rect.Height);
                var v3 = (rect.Pos[0], rect.Pos[1] - rect.Height);
                bb.Add(new Triangle(v0, v1, v2, rectColor));
                bb.Add(new Triangle(v2, v3, v0, rectColor));
            }
            foreach (var line in root.Back.Lines)
            {
                Color lineColor = ColorTranslator.FromHtml(line.Color);
                var (v0, v1, v2, v3) = GetLineRectangle(
                    line.Start[0], line.Start[1],
                    line.End[0], line.End[1],
                    line.Thickness
                );
                bb.Add(new Triangle(v0, v1, v2, lineColor));
                bb.Add(new Triangle(v2, v3, v0, lineColor));
            }
            return bb;
        }
    }


}
