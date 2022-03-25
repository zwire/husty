using System;
using System.Text.Json.Serialization;
using static System.Math;

namespace Husty.Geometry
{

    public enum AngleType { Radian, Degree }

    public struct Angle : IEquatable<Angle>
    {

        // ------ fields ------ //

        private readonly double _radian;


        // ------ properties ------ //

        public double Radian => _radian;

        public double Degree => _radian * 180 / PI;

        public static Angle Zero => new(0, AngleType.Radian);


        // ------ constructors ------ //

        public Angle(double value, AngleType type)
        {
            if (type is AngleType.Radian && (value < -PI || value > PI))
                throw new ArgumentOutOfRangeException("must be -PI <= value <= PI");
            if (type is AngleType.Degree && (value < -180 || value > 180))
                throw new ArgumentOutOfRangeException("must be -180 <= value <= 180");
            _radian = type is AngleType.Degree ? value * PI / 180 : value;
        }

        [JsonConstructor]
        public Angle(double radian = 0, double degree = 0)
        {
            _radian = radian;
        }

        public Angle Add(double value, AngleType type) => new(RegulateRange(_radian + value), type);

        public Angle Subtract(double value, AngleType type) => new(RegulateRange(_radian - value), type);

        public bool Equals(Angle obj) => GetHashCode() == obj.GetHashCode();

        public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

        public override int GetHashCode() => new { Radian }.GetHashCode();


        // ------ operators ------ //

        public static Angle operator +(Angle a, Angle b) => a.Add(b.Radian, AngleType.Radian);

        public static Angle operator -(Angle a, Angle b) => a.Subtract(b.Radian, AngleType.Radian);

        public static Angle operator -(Angle a) => new(-a.Radian, AngleType.Radian);

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
}
