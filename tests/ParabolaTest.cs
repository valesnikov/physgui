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

    private static (double, double) expected((double x, double y) mov, double time)
    {
        return (mov.x * time, mov.y * time + ACCELERATION * time * time / 2);
    }

    [Fact]
    public void test()
    {
        Random rand = new Random();
        int length = rand.Next(1, 101);
        (double x, double y)[] movs = new (double, double)[length];

        using var phys = new physgui.PhysicsSystem(length);
        phys.AccelerationOfGravity.Y = ACCELERATION;

        for (int i = 0; i < length; i++)
        {
            var speed = rand.NextDouble() * 99 + 1;
            var angle = rand.NextDouble() * 2 * Math.PI;
            movs[i] = (speed * Math.Cos(angle), speed * Math.Sin(angle));

            var obj = phys.Objects[i];
            obj.Mass = 1;
            obj.Radius = 0.03;
            obj.Movement.X = movs[i].x;
            obj.Movement.Y = movs[i].y;
        }

        for (int j = 0; j < 10; j++)
        {
            phys.Run(ACCURACY, 1);
            for (int i = 0; i < length; i++)
            {
                var obj = phys.Objects[i];
                var (x, y) = expected(movs[i], phys.Time);
                Assert.True(distance(obj.Position, x, y) < 0.001);
            }
        }
    }
}

