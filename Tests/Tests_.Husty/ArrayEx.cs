using Husty.Extensions;

namespace Tests_.Husty;

public class ArrayEx
{
    [Fact]
    public void Cast()
    {
        var first = new[] { 1, 2, 3, 4 };
        first.AsByteArray();
        first.AsShortArray();
        first.AsIntArray();
        first.AsFloatArray();
        first.AsDoubleArray();
        var second = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
        second.AsByteArray();
        second.AsShortArray();
        second.AsIntArray();
        second.AsFloatArray();
        second.AsDoubleArray();
        var second2 = new[,] { { 1, 2 }, { 3, 4 } };
        second2.AsByteArray();
        second2.AsShortArray();
        second2.AsIntArray();
        second2.AsFloatArray();
        second2.AsDoubleArray();
        var third = new[] { new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, new[] { 7, 8, 9 } } };
        third.AsByteArray();
        third.AsShortArray();
        third.AsIntArray();
        third.AsFloatArray();
        third.AsDoubleArray();
        var third2 = new[, ,] { { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } } };
        third2.AsShortArray();
        third2.AsIntArray();
        third2.AsFloatArray();
        third2.AsDoubleArray();
    }

    [Fact]
    public void Reshape()
    {
        var first = new[] { 1, 2, 3, 4, 5, 6 };
        first.To2DArray(2, 3);
        first.To2DJaggedArray(2, 3);
        first.To3DArray(1, 2, 3);
        first.To3DJaggedArray(1, 2, 3);
        var second = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
        second.To1DArray();
        second.To2DArray();
        second.Transpose();
        var second2 = new[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 } };
        second2.To1DArray();
        second2.To2DJaggedArray();
        second2.Transpose();
        var third = new[] { new[] { new[] { 1, 2, 3, 4 }, new[] { 4, 5, 6, 7 }, new[] { 7, 8, 9, 10 } } };
        third.To1DArray();
        third.To3DArray();
        third.Transpose(0, 1, 2);
        third.Transpose(0, 2, 1);
        third.Transpose(1, 0, 2);
        third.Transpose(1, 2, 0);
        third.Transpose(2, 0, 1);
        third.Transpose(2, 1, 0);
        var third2 = new[, ,] { { { 1, 2, 3, 4 }, { 4, 5, 6, 7 }, { 7, 8, 9, 10 } } };
        third2.To1DArray();
        third2.To3DJaggedArray();
        third2.Transpose(0, 1, 2);
        third2.Transpose(0, 2, 1);
        third2.Transpose(1, 0, 2);
        third2.Transpose(1, 2, 0);
        third2.Transpose(2, 0, 1);
        third2.Transpose(2, 1, 0);
    }

}