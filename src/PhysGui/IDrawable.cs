namespace PhysGui
{
    public interface IDrawable
    {
        // these functions are called when there is an existing GL context.
        void Realized();
        void Render(double centerX, double centerY, double scale, double aspect);
        void Unrealized();
    }
}