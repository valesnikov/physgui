namespace tests;

public class MoonOrbitTests
{
    private const double MOON_MASS = 7.3477e22;
    private const double MOON_RADIUS = 1737100;
    private const double HEIGHT = 1;
    private const double ACCURACY = 0.001;

    private static double FirstCosmicVelocity(double mass, double radius)
    {
        return Math.Sqrt(physgui.LibFlPhys.PHYS_G * mass / radius);
    }

    private static double distance(physgui.Vector a, double x, double y)
    {
        double dx = a.X - x;
        double dy = a.Y - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    [Fact]
    public void test()
    {
        using var phys = new physgui.PhysicsSystem(2);
        phys.IsGravityEnabled = true;

        var moon = phys.Objects[0];
        moon.Mass = MOON_MASS;
        moon.Radius = MOON_RADIUS;

        var rock = phys.Objects[1];
        rock.Mass = 1;
        rock.Radius = 0.03;
        rock.Position.Y = MOON_RADIUS + HEIGHT;
        rock.Movement.X = FirstCosmicVelocity(MOON_MASS, MOON_RADIUS + HEIGHT);

        double expectedTime = (2 * physgui.LibFlPhys.PHYS_PI * (MOON_RADIUS + HEIGHT)) / rock.Movement.Length;

        uint steps = (uint)(expectedTime / ACCURACY / 4);

        var res = phys.Run(ACCURACY, steps);
        Assert.Equal(physgui.LibFlPhys.PHYS_RES_OK, res);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, MOON_RADIUS + HEIGHT, 0) <= 12);

        res = phys.Run(ACCURACY, steps);
        Assert.Equal(physgui.LibFlPhys.PHYS_RES_OK, res);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, 0, -(MOON_RADIUS + HEIGHT)) <= 25);

        res = phys.Run(ACCURACY, steps);
        Assert.Equal(physgui.LibFlPhys.PHYS_RES_OK, res);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, -(MOON_RADIUS + HEIGHT), 0) <= 50);

        res = phys.Run(ACCURACY, steps);
        Assert.Equal(physgui.LibFlPhys.PHYS_RES_OK, res);
        Assert.True(distance(moon.Position, 0, 0) <= 1);
        Assert.True(distance(rock.Position, 0, MOON_RADIUS + HEIGHT) <= 100);
    }
}

