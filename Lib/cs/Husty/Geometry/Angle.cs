using System;
using static System.Math;

namespace Husty
{

    public enum AngleType { Radian, Degree }

    public class Angle
    {

        // ------ fields ------ //

        private readonly double _radian;


        // ------ properties ------ //

        public double Radian => _radian;

        public double Degree => _radian * 180 / PI;


        // ------ constructors ------ //

        public Angle(double value, AngleType type)
        {
            if (type is AngleType.Radian && (value < -PI || value > PI)) throw new ArgumentOutOfRangeException("value must be -PI~PI");
            if (type is AngleType.Degree && (value < -180 || value > 180)) throw new ArgumentOutOfRangeException("value must be -180~180");
            _radian = type is AngleType.Degree ? value * PI / 180 : value;
        }

        public Angle Plus(double value, AngleType type) => new(RegulateRange(_radian + value), type);

        public Angle Minus(double value, AngleType type) => new(RegulateRange(_radian - value), type);


        // ------ operators ------ //

        public static Angle operator +(Angle a1, Angle a2) => a1.Plus(a2.Radian, AngleType.Radian);

        public static Angle operator -(Angle a1, Angle a2) => a1.Minus(a2.Radian, AngleType.Radian);

        public static Angle operator -(Angle a) => new(-a.Radian, AngleType.Radian);


        // ------ private methods ------ //

        private static double RegulateRange(double radian)
        {
            if (radian > PI) return radian - PI * 2;
            if (radian < -PI) return radian + PI * 2;
            return radian;
        }

    }
}
