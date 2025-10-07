namespace tests;

public class ParabolaTest
{
    private const double ACCURACY = 0.1;

    private const double ACCELERATION = -physgui.LibFlPhys.PHYS_ACCEL_OF_FREE_FALL;

    private static double distance(physgui.Vector a, double x, double y)
    {
        double dx = a.X - x;
        double dy = a.Y - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static (double, double) expected((double, double) mov, double time)
    {
        var (vx, vy) = mov;
        return (vx * time, vy * time + ACCELERATION * time * time / 2);
    }

    [Fact]
    public void test()
    {
        Random rand = new Random();
        int length = rand.Next(1, 101);
        (double, double)[] movs = new (double, double)[length];

        using var phys = new physgui.PhysicsSystem((uint)length);
        phys.AccelerationOfGravity.Y = ACCELERATION;

        for (int i = 0; i < length; i++)
        {
            double speed = rand.NextDouble() * 99 + 1;
            double angle = rand.NextDouble() * 2 * Math.PI;
            movs[i] = (speed * Math.Cos(angle), speed * Math.Sin(angle));

            var obj = phys.Objects[i];
            obj.Mass = 1;
            obj.Radius = 0.03;
            obj.Movement.X = movs[i].Item1;
            obj.Movement.Y = movs[i].Item2;
        }

        for (int j = 0; j < 10; j++)
        {
            phys.Run(ACCURACY, 1);
            var time = phys.Time;
            for (int i = 0; i < length; i++)
            {
                var obj = phys.Objects[i];
                var (x, y) = expected(movs[i], time);
                Assert.True(distance(obj.Position, x, y) < 0.001);
            }
        }
    }
}

