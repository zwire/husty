using System.Text.Json.Serialization;
using static System.Math;

namespace Husty.Geometry;

public struct Angle : IEquatable<Angle>
{

    // ------ fields ------ //

    private readonly double _radian;


    // ------ properties ------ //

    public double Radian => _radian;

    public double Degree => _radian * 180 / PI;

    public static Angle Zero => new(0);

    public static Angle MinValue => new(-PI + 1e-16);

    public static Angle MaxValue => new(PI - 1e-16);


    // ------ constructors ------ //

    [JsonConstructor]
    public Angle(double radian)
    {
        if (radian < -PI || radian > PI)
            throw new ArgumentOutOfRangeException("must be -PI <= value <= PI");
        _radian = radian;
    }


    // ------ factory methods ------ //

    public static Angle FromRadian(double value) => new(value);

    public static Angle FromDegree(double value) => new(value * PI / 180);


    // ------ public methods ------ //

    public bool Equals(Angle obj) => GetHashCode() == obj.GetHashCode();

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

    public override int GetHashCode() => new { Radian }.GetHashCode();


    // ------ operators ------ //

    public static Angle operator +(Angle a, Angle b) => new(RegulateRange(a.Radian + b.Radian));

    public static Angle operator -(Angle a, Angle b) => new(RegulateRange(a.Radian - b.Radian));

    public static Angle operator -(Angle a) => new(-a.Radian);

    public static bool operator ==(Angle a, Angle b) => a.Equals(b);

    public static bool operator !=(Angle a, Angle b) => !a.Equals(b);

    public static bool operator <(Angle a, Angle b) => a.Radian < b.Radian;

    public static bool operator >(Angle a, Angle b) => a.Radian > b.Radian;

    public static bool operator <=(Angle a, Angle b) => a.Radian <= b.Radian;

    public static bool operator >=(Angle a, Angle b) => a.Radian >= b.Radian;


    // ------ private methods ------ //

    private static double RegulateRange(double radian)
    {
        if (radian > PI) return radian - PI * 2;
        if (radian < -PI) return radian + PI * 2;
        return radian;
    }

}
