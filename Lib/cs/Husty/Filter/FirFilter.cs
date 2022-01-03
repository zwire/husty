using System;

namespace Husty
{
    /// <summary>
    /// Finite Impulse Response
    /// See http://www.densikairo.com/Development/Public/study_dsp/464952A5D5A5A3A5EBA5BFA4CBB4D8A4B7A4C6.html
    /// </summary>
    public sealed class FirFilter
    {

        // ------ fields ------ //

        private int _count;
        private readonly double[] _buffer;
        private readonly double[] _weightTable;


        // ------ constructors ------ //

        public FirFilter(int size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException("Require: size > 0");
            _buffer = new double[size];
            _weightTable = new double[size];
            var one = 1.0 / size;
            for (int i = 0; i < size; i++)
                _weightTable[i] = one;
        }

        public FirFilter(double[] weightTable)
        {
            _buffer = new double[weightTable.Length];
            _weightTable = weightTable;
        }


        // ------ public methods ------ //

        public double Update(double measurementValue)
        {
            var len = _buffer.Length;
            for (int i = 0; i < len - 1; i++)
                _buffer[i] = _buffer[i + 1];
            _buffer[^1] = measurementValue;
            if (++_count < len) return measurementValue;
            if (_count % len is 0) _count = len;

            var value = 0.0;
            for (int i = 0; i < len; i++)
                value += _buffer[i] * _weightTable[i];
            return value;
        }

    }
}
