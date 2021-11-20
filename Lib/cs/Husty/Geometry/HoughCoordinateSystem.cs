namespace Husty
{
    public record HoughCoordinateSystem(Angle Theta, double Rho)
    {

        public Line2D ToLine2D() => new(this);

    }
}
