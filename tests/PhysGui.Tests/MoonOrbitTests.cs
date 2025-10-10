namespace tests;

public class MoonOrbitTests
{
    private const double MOON_MASS = 7.3477e22;
    private const double MOON_RADIUS = 1737100;
    private const double HEIGHT = 1;
    private const double ACCURACY = 0.001;

    private static double FirstCosmicVelocity(double mass, double radius)
    {
        return Math.Sqrt(PhysGui.LibFlPhys.PHYS_G * mass / radius);
    }

    private static double distance(PhysGui.Vector a, double x, double y)
    {
        double dx = a.X - x;
        double dy = a.Y - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    [Fact]
    public void test()
    {
        using var phys = new PhysGui.PhysicsSystem(2);
        phys.IsGravityEnabled = true;

        var moon = phys.Objects[0];
        moon.Mass = MOON_MASS;
        moon.Radius = MOON_RADIUS;

        var rock = phys.Objects[1];
        rock.Mass = 1;
        rock.Radius = 0.03;
        rock.Position.Y = MOON_RADIUS + HEIGHT;
        rock.Movement.X = FirstCosmicVelocity(MOON_MASS, MOON_RADIUS + HEIGHT);

        double expectedTime = (2 * PhysGui.LibFlPhys.PHYS_PI * (MOON_RADIUS + HEIGHT)) / rock.Movement.Length;

        long steps = (long)(expectedTime / ACCURACY / 4);

        phys.Run(ACCURACY, steps);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, MOON_RADIUS + HEIGHT, 0) <= 12.5);

        phys.Run(ACCURACY, steps);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, 0, -(MOON_RADIUS + HEIGHT)) <= 25);

        phys.Run(ACCURACY, steps);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, -(MOON_RADIUS + HEIGHT), 0) <= 50);

        phys.Run(ACCURACY, steps);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, 0, MOON_RADIUS + HEIGHT) <= 100);
    }
}
