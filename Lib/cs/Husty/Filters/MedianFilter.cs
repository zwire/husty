namespace Husty.Filters;

public sealed class MedianFilter
{

    // ------ fields ------ //

    private int _count;
    private readonly double[] _buffer;


    // ------ constructors ------- //

    public MedianFilter(int size)
    {
        if (size < 1) throw new ArgumentOutOfRangeException("Require: size must be > 0");
        _buffer = new double[size];
    }


    // ------ public methods ------ //

    public double Update(double measurementValue)
    {
        var len = _buffer.Length;
        _buffer[_count++ % len] = measurementValue;
        if (_count < len) return measurementValue;
        if (_count % len is 0) _count = len;

        var val = _buffer.OrderBy(x => x).ToArray();
        if (len % 2 is 0)
            return (val[len / 2 - 1] + val[len / 2]) / 2;
        else
            return val[len / 2];
    }

}
