using System;
using System.Linq;

namespace Husty
{
    public class MultidimensionalArray
    {

        // ------ fields ------ //

        private readonly int[] _vectorSizes;
        private readonly int[] _dimSizes;
        private double[] _values;


        // ------ constructor ------ //

        public MultidimensionalArray(int[] vectorSizes)
        {
            if (vectorSizes.Any(x => x <= 0))
                throw new ArgumentException("all vector sizes must be positive value.");
            _vectorSizes = vectorSizes;
            _values = new double[_vectorSizes.Aggregate(1, (s, v) => s *= v)];
            _dimSizes = new int[_vectorSizes.Length];
            for (int i = _vectorSizes.Length - 1; i > -1; i--)
                _dimSizes[i] = _vectorSizes.Take(i).Aggregate(1, (s, v) => s *= v);
        }


        // ------ public methods ------ //

        public int GetLength(int dimensionIndex)
        {
            return _vectorSizes[dimensionIndex];
        }

        public int GetTotalSize()
        {
            return _values.Length;
        }

        public void SetAt(int[] location, double value)
        {
            if (location.Length != _vectorSizes.Length)
                throw new ArgumentException(nameof(location));
            var loc1D = FlattenLocation(location);
            _values[loc1D] = value;
        }

        public void SetAll(double[] values)
        {
            if (_values.Length != values.Length)
                throw new ArgumentException(nameof(values));
            _values = values;
        }

        public double GetAt(int[] location)
        {
            if (location.Length != _vectorSizes.Length)
                throw new ArgumentException(nameof(location));
            var loc1D = FlattenLocation(location);
            return _values[loc1D];
        }

        public double[] GetAll()
        {
            return _values;
        }

        public void PlusAt(int[] location, double value)
        {
            var loc1D = FlattenLocation(location);
            _values[loc1D] += value;
        }

        public void MinusAt(int[] location, double value)
        {
            var loc1D = FlattenLocation(location);
            _values[loc1D] -= value;
        }

        public void MulAt(int[] location, double value)
        {
            var loc1D = FlattenLocation(location);
            _values[loc1D] *= value;
        }

        public void DivAt(int[] location, double value)
        {
            var loc1D = FlattenLocation(location);
            _values[loc1D] /= value;
        }


        // ------ private methods ------ //

        private int FlattenLocation(int[] location)
        {
            var pos = 0;
            for (int i = 0; i < _dimSizes.Length; i++)
            {
                if (location[i] < 0 || location[i] > _vectorSizes[i] - 1)
                    throw new ArgumentOutOfRangeException(nameof(location));
                pos += location[i] * _dimSizes[i];
            }
            return pos;
        }

    }
}
