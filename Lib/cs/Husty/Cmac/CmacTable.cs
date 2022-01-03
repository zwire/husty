using System;
using System.Collections.Generic;
using System.Linq;

namespace Husty.Cmac
{
    internal class CmacTable
    {

        // ------ fields ------ //

        private readonly int[] _activeLocation;
        private readonly double _min, _max;
        private readonly double[] _offsets, _steps;
        private readonly MultidimensionalArray _array;


        // ------ properties ------ //

        public int DimensionCount => _activeLocation.Length;

        public int[] ActiveLocationIndex => _activeLocation;

        public double ActiveValue => _array.GetAt(_activeLocation);


        // ------ constructors ------ //

        public CmacTable(
            IEnumerable<CmacLabelInfo> labelInfos,
            double min, double max,
            double initialValue
        ) : this(
            labelInfos,
            min, max,
            Enumerable.Repeat(initialValue, labelInfos.Aggregate(1, (s, v) => s *= v.GridCount)).ToArray()
        )
        { }

        public CmacTable(
            IEnumerable<CmacLabelInfo> labelInfos,
            double min, double max,
            double[] initialValues
        )
        {
            _min = min;
            _max = max;

            var infos = labelInfos.ToArray();
            var dims = new int[infos.Length];
            _offsets = new double[infos.Length];
            _steps = new double[infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                dims[i] = infos[i].GridCount;
                _offsets[i] = infos[i].Lower;
                _steps[i] = (infos[i].Upper - infos[i].Lower) / infos[i].GridCount;
            }

            _array = new(dims);
            if (initialValues.Length != _array.GetTotalSize())
                throw new ArgumentException("input values length is invalid.");
            _array.SetAll(initialValues);
            _activeLocation = new int[infos.Length];
        }


        // ------ public methods ------ //

        public void FixLocation(double[] state)
        {
            for (int i = 0; i < _activeLocation.Length; i++)
            {
                var maxIndex = _array.GetLength(i);
                _activeLocation[i] = maxIndex - 1;
                for (int x = 0; x < maxIndex; x++)
                {
                    if (state[i] < _offsets[i] + x * _steps[i])
                    {
                        _activeLocation[i] = x;
                        break;
                    }
                }
            }
        }

        public void ApplyPenalty(double value)
        {
            _array.MinusAt(_activeLocation, value);
            if (_array.GetAt(_activeLocation) < _min)
                _array.SetAt(_activeLocation, _min);
            else if (_array.GetAt(_activeLocation) > _max)
                _array.SetAt(_activeLocation, _max);
        }

        public double[] GetParams() => _array.GetAll();

        public void SetParams(double[] value) => _array.SetAll(value);

    }
}
