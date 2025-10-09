using System;

namespace tests;

public class GetSetTests
{
    [Fact]
    public void test()
    {
        Random rand = new Random();

        using var phys = new physgui.PhysicsSystem(1);

        double val;
        phys.Density = val = rand.NextDouble();
        Assert.Equal(val, phys.Density);

        phys.AccelerationOfGravity.X = val = rand.NextDouble();
        Assert.Equal(val, phys.AccelerationOfGravity.X);

        phys.AccelerationOfGravity.Y = val = rand.NextDouble();
        Assert.Equal(val, phys.AccelerationOfGravity.Y);

        phys.Wind.X = val = rand.NextDouble();
        Assert.Equal(val, phys.Wind.X);

        phys.Wind.Y = val = rand.NextDouble();
        Assert.Equal(val, phys.Wind.Y);

        var obj = phys.Objects[0];

        obj.Position.X = val = rand.NextDouble();
        Assert.Equal(val, obj.Position.X);

        obj.Position.Y = val = rand.NextDouble();
        Assert.Equal(val, obj.Position.Y);

        obj.Movement.X = val = rand.NextDouble();
        Assert.Equal(val, obj.Movement.X);

        obj.Movement.Y = val = rand.NextDouble();
        Assert.Equal(val, obj.Movement.Y);

        obj.Mass = val = rand.NextDouble();
        Assert.Equal(val, obj.Mass);

        obj.Radius = val = rand.NextDouble();
        Assert.Equal(val, obj.Radius);

        Assert.Equal(0, phys.Time);
        phys.Run(val = rand.NextDouble(), 1);
        Assert.Equal(val, phys.Time);
    }
}
